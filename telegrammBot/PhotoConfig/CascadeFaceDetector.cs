﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace telegrammBot.PhotoConfig
{
    public class CascadeFaceDetector : DisposableObject
    {
        public CascadeFaceDetector()
        {
            Init().Wait();
        }

        private static CascadeClassifier _faceCascadeClassifier = null;

        /// <summary>
        /// Detect faces and eyes region from the input image
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="faces">The region of the faces.</param>
        public void Detect(
            IInputArray image,
            List<Rectangle> faces)
        {
            using (Mat gray = new Mat())
            {
                CvInvoke.CvtColor(image, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(gray, gray);

                //Detect the faces from the gray scale image and store the locations as rectangle
                //The first dimensional is the channel
                //The second dimension is the index of the rectangle in the specific channel                     
                Rectangle[] facesDetected = _faceCascadeClassifier.DetectMultiScale(
                    gray,
                    1.1,
                    10,
                    new Size(20, 20));
                faces.AddRange(facesDetected);
            }
            
        }


        /// <summary>
        /// Download and initialize the face and eye cascade classifier detection model
        /// </summary>
        /// <param name="onDownloadProgressChanged">Call back method during download</param>
        /// <param name="initOptions">Initialization options. None supported at the moment, any value passed will be ignored.</param>
        /// <returns>Asyn task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator Init(DownloadProgressChangedEventHandler onDownloadProgressChanged = null, Object initOptions = null)
#else
        public async Task Init(DownloadProgressChangedEventHandler onDownloadProgressChanged = null, Object initOptions = null)
#endif
        {
            if (_faceCascadeClassifier == null)
            {
                FileDownloadManager downloadManager = new FileDownloadManager();
                String url = "https://github.com/opencv/opencv/raw/4.2.0/data/haarcascades/";
                downloadManager.AddFile(url + "/haarcascade_frontalface_default.xml", "haarcascade");

                downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return downloadManager.Download();
#else
                await downloadManager.Download();
#endif
                String faceFile = downloadManager.Files[0].LocalFile;
                _faceCascadeClassifier = new CascadeClassifier(faceFile);
                
            }
        }

        private MCvScalar _renderColorFace = new MCvScalar(0, 0, 255);

        /// <summary>
        /// Get or Set the color used in rendering face.
        /// </summary>
        public MCvScalar RenderColorFace
        {
            get
            {
                return _renderColorFace;
            }
            set
            {
                _renderColorFace = value;
            }
        }

        private MCvScalar _renderColorEye = new MCvScalar(255, 0, 0);


        /// <summary>
        /// Process the input image and render into the output image
        /// </summary>
        /// <param name="imageIn">The input image</param>
        /// <param name="imageOut">
        /// The output image, can be the same as <paramref name="imageIn"/>, in which case we will render directly into the input image.
        /// Note that if no faces are detected, <paramref name="imageOut"/> will remain unchanged.
        /// If faces/eyes are detected, we will draw the (rectangle) regions on top of the existing pixels of <paramref name="imageOut"/>.
        /// If the <paramref name="imageOut"/> is not the same object as <paramref name="imageIn"/>, it is a good idea to copy the pixels over from the input image before passing it to this function.
        /// </param>
        /// <returns>The messages that we want to display.</returns>
        public int GetFacesCount(IInputArray imageIn, IInputOutputArray imageOut)
        {
            List<Rectangle> faces = new List<Rectangle>();
            //List<Rectangle> eyes = new List<Rectangle>();

            //Stopwatch watch = Stopwatch.StartNew();
            Detect(imageIn, faces);
            //watch.Stop();

            //Draw the faces
            foreach (Rectangle rect in faces)
             CvInvoke.Rectangle(imageOut, rect, RenderColorFace, 2);

            //Draw the eyes 
            //foreach (Rectangle rect in eyes)
            //    CvInvoke.Rectangle(imageOut, rect, RenderColorEye, 2);

            return faces.Count;
        }

        /// <summary>
        /// Clear and reset the model. Required Init function to be called again before calling ProcessAndRender.
        /// </summary>
        public void Clear()
        {
            if (_faceCascadeClassifier != null)
            {
                _faceCascadeClassifier.Dispose();
                _faceCascadeClassifier = null;
            }
        }

        /// <summary>
        /// Release the memory associated with this face and eye detector
        /// </summary>
        protected override void DisposeObject()
        {
            Clear();
        }
    }
}