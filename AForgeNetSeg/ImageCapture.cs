using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;

using AForge.Video;

namespace AForgeNetSeg
{
    class ImageCapture
    {
        private MJPEGStream video;

        public void captureImage()
        {
            video = new MJPEGStream("http://147.232.24.183/cgi-bin/viewer/video.jpg");
            video.Login = "viewer";
            video.Password = "";
            video.NewFrame += new NewFrameEventHandler(video_NewFrame);
            video.Start();
        }

        private void video_NewFrame(object sender,
                NewFrameEventArgs eventArgs)
        {
            // get new frame
            Bitmap bitmap = eventArgs.Frame;
            // process the frame
            bitmap.Save("from_camera", ImageFormat.Png);
            video.Stop();
        }

        public void getCamImage(string root)
        {
            int read, total = 0;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(root);
            req.Credentials = new NetworkCredential("viewer", "");

            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            byte[] buffer = new byte[50000];

            while ((read = stream.Read(buffer, total, 1000)) != 0)
            {
                total += read;
            }
            Bitmap bmp;

            using (var ms = new MemoryStream(buffer, 0, total))
            {
                ms.Seek(0, SeekOrigin.Begin);
                byte[] buf = ms.ToArray();

                bmp = new Bitmap(ms);
            }

            bmp.Save("test", ImageFormat.Bmp);
        }
    }
}
