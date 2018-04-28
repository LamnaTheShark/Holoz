using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;
using Android.Content;
using Xamarin.Forms;
using Android.Graphics;
using static Android.Provider.MediaStore.Images;
using Android.Util;
using Java.Util;
using System.Collections.Generic;
using System.Threading;
using Java.Util.Logging;
using System.Runtime.InteropServices;
using Java.Lang;
using System.Threading.Tasks;

using Android.Graphics.Drawables;

using Android.Graphics.Drawables.Shapes;

using System.IO;

namespace Holoz.Droid
{

    [Activity(Label = "Holoz", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static readonly int PickImageId = 1000;  
        private ImageView _imageView;

        public int Xresolution;
        public int Yresolution;
        public int Xrect;
        public int Yrect;
        public double stretch;
        public double size;
        public double speed;
        public double delay;
        public bool loop;
        private double pps;
        private double frameDelay;
        Android.Net.Uri Image;

        TextView stretchText;
        TextView sizeText;
        TextView speedText;
        TextView delayText;

        SeekBar speedSli;
        SeekBar stretchSli;
        SeekBar sizeSli;
        SeekBar delayBa;

        TextView imageloc;

        ImageView renderPreview;

        //ImageView renderLay;

        protected override void OnCreate(Bundle bundle)
        {
            //TabLayoutResource = Resource.Layout.MainLayout;

            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainLayout);
            _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            Android.Widget.Button button = FindViewById<Android.Widget.Button>(Resource.Id.MyButton);
            button.Click += ButtonOnClick;

            Android.Widget.Button startbutton = FindViewById<Android.Widget.Button>(Resource.Id.startButton);
            startbutton.Click += StartButtonOnClickAsync;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            imageloc = FindViewById<TextView>(Resource.Id.imgLocat);

            stretchText = FindViewById<TextView>(Resource.Id.textStretch);
            sizeText = FindViewById<TextView>(Resource.Id.textSize);
            speedText = FindViewById<TextView>(Resource.Id.textSpeed);
            delayText = FindViewById<TextView>(Resource.Id.textDelay);

            speedSli = FindViewById<SeekBar>(Resource.Id.speedSlider);
            speedSli.ProgressChanged += speedChanged;
            stretchSli = FindViewById<SeekBar>(Resource.Id.stretchSlider);
            stretchSli.ProgressChanged += stretchChanged;
            sizeSli = FindViewById<SeekBar>(Resource.Id.sizeSlider);
            sizeSli.ProgressChanged += sizeChanged;

            delayBa = FindViewById<SeekBar>(Resource.Id.seekDelay);
            delayBa.ProgressChanged += delayChanged;

            renderPreview = FindViewById<ImageView>(Resource.Id.imageRender);
            renderPreview.Enabled = false;
            renderPreview.Visibility = ViewStates.Invisible;
            delay = 0;
            speed = 0;
            //renderLay = FindViewById<ImageView>(Resource.Id.renderImg);
            //renderLay.Enabled = false;

            //NavigationPage.SetHasNavigationBar(, false);
        }



        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }

        void speedChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {  
            speed =  e.Progress;
            speedText.Text = speed.ToString();
        }

        void stretchChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            stretch = ((double)e.Progress)/100;
            stretchText.Text = stretch.ToString();
        }

        void sizeChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            size = ((double)e.Progress)/100;
            sizeText.Text = size.ToString();
        }

        void delayChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            delay = ((double)e.Progress);
            delayText.Text = delay.ToString();
        }


        private async void StartButtonOnClickAsync(object sender, EventArgs eventArgs)
        {
            //We start the render
            //imageloc.Text = " d1 " + pps + "pps; " + Yrect + "Yrect; " + Xrect + "Xrect; ";
            Bitmap mBitmap = null;
            mBitmap = Media.GetBitmap(this.ContentResolver, Image);
            DisplayMetrics metric = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(metric);
            pps = mBitmap.Width / speed;
            frameDelay = mBitmap.Width / pps;
 
            Yrect = (int)(stretch * metric.HeightPixels);
            Xrect = (int)(size * metric.WidthPixels);

            //imageloc.Text = " d2 " + pps + "pps; " + Yrect + "Yrect; " + Xrect + "Xrect; ";

            if (Yrect <= 0 ||  Xrect <= 0)
            {
                imageloc.Text = " e1 " + "SIZE ERROR" + pps + "pps; " + Yrect + "Yrect; " + Xrect + "Xrect; " + mBitmap.Height + "bitH; " + mBitmap.Width + "bitW; ";
                return;
                
            }
            
            List<int[]> bar = new List<int[]>();

            Bitmap lImage = Bitmap.CreateScaledBitmap(mBitmap, mBitmap.Width, Yrect, true);
            Bitmap.Config conf = Bitmap.Config.Argb8888; // see other conf types
            Bitmap Render = Bitmap.CreateBitmap(metric.WidthPixels, Yrect, conf);
            //imageloc.Text = " d3 " + frameDelay + "frameDelay; " + lImage.Width + "newBitmapWidth; " + lImage.Height + "newBitmapheight; ";

            renderPreview.Enabled = true;
            renderPreview.Visibility = ViewStates.Visible;
            //Xamarin.Forms.Layout.C RaiseChild(Resource.Layout.renderLayout);
            int[] getp;
            Render.SetPixel(0,0, Android.Graphics.Color.Black);
            renderPreview.SetImageBitmap(Render);
            if (delay != 0)
            {
                await Task.Delay((int)delay*1000);
            }

            for (int x = 0; x < lImage.Width; x++)
            {
                if (speed != 0)
                {
                    await Task.Delay((int)speed);
                }
                else
                {
                    await Task.Delay((int)1);
                }
                //Render.Recycle();
                //Render = Bitmap.CreateBitmap(lImage.Width, Yrect, conf);
                getp = new int[lImage.Height];
                //+ lImage.Width + " lImageW; " + Render.Height + " renderH; " + Render.Width + " RenderW;" + getp.Length + " sizeGetp; ";
                lImage.GetPixels(getp, 0, 1, x, 0, 1, lImage.Height);
                for (int j = 0; j < Xrect; j++)
                {
                    Render.SetPixels(getp, 0, 1, j, 0, 1, Yrect - 1);
                }
                //int xpu = (x + 1);
                //imageloc.Text = "d5: " + xpu + " x + width; " + Render.Height + " renderH; " + Render.Width + " RenderW;" + lImage.Width + " x; ";
                //renderPreview.SetImageBitmap(Render);
                renderPreview.SetImageBitmap(Render);
            }
            Render.Recycle();
            Render = Bitmap.CreateBitmap(lImage.Width, Yrect, conf);
            renderPreview.SetImageBitmap(Render);
            if (delay != 0)
            {
                await Task.Delay((int)delay * 1000);
            }
            
            //imageloc.Text = " d4 RENDER DONE :  " + barsize + " bars; " + donebars + " donbars";
            renderPreview.Enabled = false;
            renderPreview.Visibility = ViewStates.Invisible;
        }




        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                Image = uri;
                _imageView.SetImageURI(uri);
               
                
                //imageloc.SetText(uri.ToString());

            }
        }


        }
    }

