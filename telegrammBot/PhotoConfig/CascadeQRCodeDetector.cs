using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;

namespace telegrammBot.PhotoConfig
{
    /// <summary>
    /// QRCodeDetector model
    /// </summary>
    public class CascadeQRCodeDetector : DisposableObject
    {
        private String _modelFolderName = "wechat";

        private static WeChatQRCode _QRCodeDetectionModel = null;

        /// <summary>
        /// Download and initialize the WeChatQRCode model
        /// </summary>
        /// <param name="onDownloadProgressChanged">Call back method during download</param>
        /// <returns>Asyn task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator Init(
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#else
        public async Task Init(
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#endif
        {
            if (_QRCodeDetectionModel == null)
            {
                FileDownloadManager manager = new FileDownloadManager();
                manager.AddFile(
                    "https://github.com/WeChatCV/opencv_3rdparty/raw/wechat_qrcode/detect.prototxt",
                    _modelFolderName,
                    "E8ACFC395CAF443A47F15686A9B9207B36CB8F7E6CEB8FBAF6466665E68A9466");

                manager.AddFile(
                        "https://github.com/WeChatCV/opencv_3rdparty/raw/wechat_qrcode/detect.caffemodel",
                        _modelFolderName,
                        "CC49B8C9BABAF45F3037610FE499DF38C8819EBDA29E90CA9F2E33270F6EF809");

                manager.AddFile(
                    "https://github.com/WeChatCV/opencv_3rdparty/raw/wechat_qrcode/sr.prototxt",
                    _modelFolderName,
                    "8AE41ACBA97E8B4A8E741EE350481E49B8E01D787193F470A4C95EE1C02D5B61");

                manager.AddFile(
                    "https://github.com/WeChatCV/opencv_3rdparty/raw/wechat_qrcode/sr.caffemodel",
                    _modelFolderName,
                    "E5D36889D8E6EF2F1C1F515F807CEC03979320AC81792CD8FB927C31FD658AE3");

                if (onDownloadProgressChanged != null)
                    manager.OnDownloadProgressChanged += onDownloadProgressChanged;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return manager.Download();
#else
                await manager.Download();
#endif

                if (manager.AllFilesDownloaded)
                {
                    _QRCodeDetectionModel = new WeChatQRCode(
                        manager.Files[0].LocalFile, 
                        manager.Files[1].LocalFile,
                        manager.Files[2].LocalFile,
                        manager.Files[3].LocalFile
                        );
                }
            }
        }
        
        /// <summary>
        /// Clear and reset the model. Required Init function to be called again before calling ProcessAndRender.
        /// </summary>
        public void Clear()
        {
            if (_QRCodeDetectionModel != null)
            {
                _QRCodeDetectionModel.Dispose();
                _QRCodeDetectionModel = null;
            }
        }

        /// <summary>
        /// Release the memory associated with this WeChatQRCode detector.
        /// </summary>
        protected override void DisposeObject()
        {
            Clear();
        }


        /// <summary>
        /// Download and initialize the WeChatQRCode model
        /// </summary>
        /// <param name="onDownloadProgressChanged">Callback when download progress has been changed</param>
        /// <param name="initOptions">A string, not used right now.</param>
        /// <returns>Async task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator Init(DownloadProgressChangedEventHandler onDownloadProgressChanged = null, Object initOptions = null)
#else
        public async Task Init(DownloadProgressChangedEventHandler onDownloadProgressChanged = null, Object initOptions = null)
#endif
        {

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
            yield return Init(onDownloadProgressChanged);
#else
            await Init(onDownloadProgressChanged);
#endif
        }

        private MCvScalar _renderColor = new MCvScalar(0, 0, 255);

        /// <summary>
        /// Get or Set the color used in rendering.
        /// </summary>
        public MCvScalar RenderColor
        {
            get
            {
                return _renderColor;
            }
            set
            {
                _renderColor = value;
            }
        }

        /// <summary>
        /// Process the input image and render into the output image
        /// </summary>
        /// <param name="imageIn">The input image</param>
        /// <param name="imageOut">
        /// The output image, can be the same as <paramref name="imageIn"/>, in which case we will render directly into the input image.
        /// Note that if no qr codes are detected, <paramref name="imageOut"/> will remain unchanged.
        /// If qr codes are detected, we will draw the code and (rectangle) regions on top of the existing pixels of <paramref name="imageOut"/>.
        /// If the <paramref name="imageOut"/> is not the same object as <paramref name="imageIn"/>, it is a good idea to copy the pixels over from the input image before passing it to this function.
        /// </param>
        /// <returns>The messages that we want to display.</returns>
        public WeChatQRCode.QRCode[] ProcessAndRender(IInputArray imageIn)
        { 
            return _QRCodeDetectionModel.DetectAndDecode(imageIn);
        }
    }
}
