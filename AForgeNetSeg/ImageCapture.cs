using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;

using AForge.Video;
using System.Net.Sockets;

namespace AForgeNetSeg
{
    class ImageCapture
    {
        private MJPEGStream video;

        public void captureImage()
        {
            video = new MJPEGStream(@"http://147.232.24.227/cgi-bin/viewer/video.jpg");
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

        public void webClientDownloadImage(string root)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(root, "test");
        }

        public void httpTesting(string root)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(root);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
        }

        public void tcpDownload(string root)
        {
            var client = new TcpClient(root, 80);
            Stream stream = client.GetStream();
        }

        public void test(string root)
        {
            int b1;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(root);
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse(); ;
            MemoryStream memoryStream = new MemoryStream();

            while ((b1 = httpWebResponse.GetResponseStream().ReadByte()) != -1) { memoryStream.WriteByte(((byte)b1)); }

            byte[] xmlBytes = memoryStream.ToArray();

            httpWebResponse.Close();

            memoryStream.Close();

            memoryStream = new MemoryStream(xmlBytes);
        }

        public bool DownloadRemoteImageFile(string uri, string fileName)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;
            request.Accept = "*/*";
            request.Headers.Add("Accept - Encoding", "gzip, deflate");
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (System.Exception)
            {
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", System.StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                return true;
            }
            else
                return false;
        }
    }
}
