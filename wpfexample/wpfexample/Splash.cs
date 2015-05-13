using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace wpfexample
{
    class Splash : IDisposable
    {
        private Form mSplash;
        private Point mLocation;

        public Splash(Point location)
        {
            mLocation = location;
            Thread t = new Thread(new ThreadStart(workerThread));
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
        public void Dispose()
        {
            mSplash.Invoke(new MethodInvoker(stopThread));
        }
        private void stopThread()
        {
            mSplash.Close();
        }
        private void workerThread()
        {
            mSplash = new SplashForm();   // Substitute this with your own
            mSplash.StartPosition = FormStartPosition.Manual;
            mSplash.Location = mLocation;
            mSplash.TopMost = true;
            Application.Run(mSplash);
        }


    }
}
