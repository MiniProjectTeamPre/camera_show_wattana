using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using ZXing;
using System.IO;
using System.Diagnostics;
using Emgu.CV.CvEnum;
using System.Collections.Generic;
using Emgu.CV.Util;
using System.Linq;

namespace camera_show {
    public partial class Form1 : Form {
        private string head = "1";
        private string steptest;
        private int process_value = 170;
        private bool flag_process = false;
        private VideoCapture capture = null;
        private Image<Bgr, Byte> img;
        private static Rectangle rect;
        private Stopwatch timeout = new Stopwatch();
        private Stopwatch timeout_show = new Stopwatch();
        private int time_out = 10000;
        private bool debug = true;
        private bool flag_set_camera = false;
        private bool steptest_fail = false;
        private int crop = 30;
        private bool steptest_camera_read2d_flag = false;
        private bool steptest_camera_matching_lcd_oo_oe_eo_ee = false;
        private bool steptest_camera_check_led_red_green = false;
        private Bgr bgr_low;
        private Bgr bgr_high;
        private Hsv hsv_low;
        private Hsv hsv_high;
        private bool flag_hsv = false;
        private Image<Hsv, Byte> img_hsv;
        private bool flag_hsv_test = false;
        private int hsv_mask = 0;
        private int hsv_timeout = 0;
        private Stopwatch stopwatch_hsv_timeout = new Stopwatch();
        private bool flag_result = false;
        private string result_blackup = "";
        private string flag_set_port = "";
        private bool flag_add_step = false;
        public Form1() {
            InitializeComponent();
            try { head = File.ReadAllText("../../config/head.txt"); } catch (Exception) { }
            try { flag_set_port = File.ReadAllText("set_port.txt"); } catch (Exception) { }
            try { flag_add_step = Convert.ToBoolean(File.ReadAllText("add_step.txt")); } catch (Exception) { }
            File.Delete("add_step.txt");
            try { steptest = File.ReadAllText("../../config/test_head_" + head + "_steptest.txt"); } catch (Exception) { }
            if (steptest == "read2d") {
                steptest_camera_read2d_flag = true;
            }
            if (flag_set_port == "set port") {
                File.Delete("set_port.txt");
                Form f2 = new Form();
                f2.Size = new Size(100, 100);
                ComboBox c = new ComboBox();
                c.Size = new Size(60, 7);
                for (int i = 0; i < 9; i++) {
                    capture = new VideoCapture(i);
                    if (capture.Width != 0) c.Items.Add(i);
                    capture.Dispose();
                }
                f2.Controls.Add(c);
                f2.ShowDialog();
                if (steptest_camera_read2d_flag == true) File.WriteAllText("../../config/test_head_" + head + "_port_read2d.txt", c.Text);
                else File.WriteAllText("../../config/test_head_" + head + "_port.txt", c.Text);
                capture = new VideoCapture(Convert.ToInt32(c.Text));
            } else {
                Stopwatch timeout_opencam = new Stopwatch();
                timeout_opencam.Restart();
                while (true) {
                    if (timeout_opencam.ElapsedMilliseconds > 5000) { capture = new VideoCapture(); MessageBox.Show("_กรุณาเลือก port camera"); break; }
                    try {
                        if (steptest_camera_read2d_flag == true) capture = new VideoCapture(Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_port_read2d.txt")));
                        else capture = new VideoCapture(Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_port.txt")));
                        if (capture.Width == 0) { DelaymS(250); continue; }
                        break;
                    } catch (Exception) {
                        DelaymS(1000);
                        //capture = new VideoCapture();
                    }
                }
                timeout_opencam.Stop();
            }
            if (capture.Width == 0) { DelaymS(200); capture = new VideoCapture(); }
            if (steptest.Contains("compar_image") || steptest.Contains("image_compar")) {
                steptest_camera_matching_lcd_oo_oe_eo_ee = true;
            }
            if (steptest == "check_led_green_on" || steptest == "check_led_red_on" || steptest == "check_led_off") {
                steptest_camera_check_led_red_green = true;
            }
            if (steptest_camera_read2d_flag == true) {
                Application.Idle += read2d;
            } else if (steptest_camera_matching_lcd_oo_oe_eo_ee == true) {
                Application.Idle += compar_image;
            } else if (steptest_camera_check_led_red_green == true) {
                Application.Idle += check_led;
            } else {
                MessageBox.Show("steptest.txt error : สเต็ปเทสที่ส่งเข้ามาในไฟล์ ไม่ตรงกับ ในตัวโปรแกรม camera.exe");
                File.WriteAllText("test_head_" + head + "_result.txt", "function\r\nFail");
                steptest_fail = true;
            }
            setup();
        }

        private void setup() {
            try {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 400);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 300);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_zoom_" + steptest + ".txt")));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Pan, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_pan_" + steptest + ".txt")));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Tilt, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_tilt_" + steptest + ".txt")));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_contrast_" + steptest + ".txt")));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_brightness_" + steptest + ".txt")));
                //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Saturation, Saturation);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt")));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_exposure_" + steptest + ".txt")));
            } catch (Exception) { }
            try { process_value = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_process_" + steptest + ".txt")); } catch (Exception) { }
            try { flag_process = Convert.ToBoolean(File.ReadAllText("../../config/test_head_" + head + "_flag_process_" + steptest + ".txt")); } catch (Exception) { }
            try { rect.X = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_x_" + steptest + ".txt")); } catch (Exception) { }
            try { rect.Y = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_y_" + steptest + ".txt")); } catch (Exception) { }
            try { rect.Width = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_width_" + steptest + ".txt")); } catch (Exception) { }
            try { rect.Height = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_height_" + steptest + ".txt")); } catch (Exception) { }
            try { debug = Convert.ToBoolean(File.ReadAllText("../../config/test_head_" + head + "_debug.txt")); } catch (Exception) { }
            try { time_out = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_timeout.txt")) * 1000; } catch (Exception) { }
            try {
                string cc = File.ReadAllText("../../config/test_head_" + head + "_bgr_" + steptest + ".txt");
                string[] zz;
                int[] xx = { 0, 0, 0, 0, 0, 0 };
                zz = cc.Split(' ');
                for (int i = 0; i < 6; i++) {
                    xx[i] = Convert.ToInt32(zz[i]);
                }
                bgr_low = new Bgr(xx[0], xx[2], xx[4]);
                bgr_high = new Bgr(xx[1], xx[3], xx[5]);
                cc = File.ReadAllText("../../config/test_head_" + head + "_hsv_" + steptest + ".txt");
                zz = cc.Split(' ');
                for (int i = 0; i < 6; i++) {
                    xx[i] = Convert.ToInt32(zz[i]);
                }
                hsv_low = new Hsv(xx[0], xx[2], xx[4]);
                hsv_high = new Hsv(xx[1], xx[3], xx[5]);
                flag_hsv = Convert.ToBoolean(File.ReadAllText("../../config/test_head_" + head + "_flag_hsv_" + steptest + ".txt"));
                hsv_mask = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_hsv_mask_" + steptest + ".txt"));
                hsv_timeout = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_hsv_timeout_" + steptest + ".txt"));
                stopwatch_hsv_timeout.Restart();
            } catch (Exception) { }
            backgroundWorker1.RunWorkerAsync();
            timeout.Restart();
        }

        private void Wattana_button_no_Click(object sender, EventArgs e) {
            File.WriteAllText("test_head_" + head + "_result.txt", "FAIL");
            this.Hide();
        }
        private void Wattana_button_yes_Click(object sender, EventArgs e) {
            File.WriteAllText("test_head_" + head + "_result.txt", "PASS");
            this.Hide();
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            int wattana_step = 0;
            backgroundWorker1.ReportProgress(9);
            while (true) {
                try {
                    wattana_step = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_wattana_step.txt"));
                } catch (Exception) { System.Threading.Thread.Sleep(100); continue; }
                File.Delete("../../config/test_head_" + head + "_wattana_step.txt");
                backgroundWorker1.ReportProgress(wattana_step);
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
            if (e.ProgressPercentage == 9) { this.Hide(); return; }
            this.Show();
            this.Controls.Clear();
            this.Controls.Add(pictureBox1);
            string wattana_step = e.ProgressPercentage.ToString();
            string wattana_form = "compar";
            string wattana_label = "_ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
            string wattana_part_image = "image1.png";
            try {
                wattana_form = File.ReadAllText("../../config/test_head_" + head + "_wattana_form.txt");
                wattana_label = File.ReadAllText("../../config/test_head_" + head + "_wattana_label.txt");
            } catch (Exception) { }
            this.Text = wattana_form;
            Label wattana_Label = new Label();
            wattana_Label.TextAlign = ContentAlignment.MiddleCenter;
            FontFamily fontFamily = new FontFamily("Arial");
            wattana_Label.Font = new Font(fontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
            wattana_Label.Text = wattana_label;
            Button wattana_button_yes = new Button();
            wattana_button_yes.Text = "yes";
            wattana_button_yes.Size = new Size(100, 50);
            wattana_button_yes.Font = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Pixel);
            wattana_button_yes.Click += Wattana_button_yes_Click;
            Button wattana_button_no = new Button();
            wattana_button_no.Text = "no";
            wattana_button_no.Size = new Size(100, 50);
            wattana_button_no.Font = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Pixel);
            wattana_button_no.Click += Wattana_button_no_Click;
            switch (wattana_step) {
                case "2":
                    this.Size = new Size((capture.Width * 2) + 30, capture.Height + 170);
                    pictureBox1.Size = new Size(capture.Width, capture.Height);
                    try { wattana_part_image = File.ReadAllText("../../config/test_head_" + head + "_wattana_part_image.txt"); } catch (Exception) { }
                    try {
                        wattana_Label.Size = new Size(720, 50);
                        wattana_Label.Location = new Point(0, 290);
                        wattana_button_no.Location = new Point(450, 350);
                        wattana_button_yes.Location = new Point(150, 350);
                        PictureBox pic = new PictureBox();
                        pic.Location = new Point(capture.Width + 10, 0);
                        pic.Size = new Size(capture.Width, capture.Height);
                        pic.Image = Bitmap.FromFile(wattana_part_image);
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;
                        this.Controls.Add(pic);
                        this.Controls.Add(wattana_button_no);
                    } catch (Exception) { }
                    break;
                case "1":
                    wattana_button_no.Location = new Point(200, 350);
                    wattana_button_yes.Location = new Point(130, 350);
                    wattana_button_yes.Text = "ok";
                    wattana_Label.Size = new Size(360, 50);
                    wattana_Label.Location = new Point(0, 290);
                    this.Size = new Size(capture.Width + 20, capture.Height + 170);
                    pictureBox1.Size = new Size(capture.Width, capture.Height);
                    break;
            }
            this.Controls.Add(wattana_button_yes);
            this.Controls.Add(wattana_Label);
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;
            int x = (w / 2) - (this.Width / 2);
            int y = (h / 2) - (this.Height / 2);
            this.Location = new Point(x, y);
            wattana_button_yes.Focus();
        }

        private void Form1_Load(object sender, EventArgs e) {
            if (steptest_fail == true) this.Close();
        }

        private void read2d(object sender, EventArgs e) {
            if (timeout.ElapsedMilliseconds >= time_out) { fail_2d(); if (debug == false) return; }
            if (IsMouseDown == true) return;
            if (capture != null && capture.Ptr != IntPtr.Zero) {
                Mat frame = capture.QueryFrame();
                try {
                    img = frame.ToImage<Bgr, Byte>();
                } catch (Exception) {
                    MessageBox.Show("ไม่สามารถเปิดกล้องได้");
                    Application.Exit();
                    return;
                }
            }
            Graphics g = Graphics.FromImage(img.Bitmap);
            g.DrawRectangle(Pens.Red, rect);
            if (flag_process == true) {
                Color img_ref;
                Bitmap img_convert;
                img_convert = new Bitmap(img.Bitmap);
                for (int i = 0; i < img_convert.Width; i++) {
                    for (int j = 0; j < img_convert.Height; j++) {
                        img_ref = img_convert.GetPixel(i, j);
                        int gg = (img_ref.R + img_ref.G + img_ref.B) / 3;
                        if (gg < process_value) img_convert.SetPixel(i, j, Color.Black);
                        else img_convert.SetPixel(i, j, Color.White);
                    }
                }
                img.Bitmap = img_convert;

                //Image<Gray, byte> imgOutput;
                //Mat hier = new Mat();
                //imgOutput = img.Convert<Gray, byte>().ThresholdBinary(new Gray(process_value), new Gray(255));
                //Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            }
            Image<Bgr, byte> img_cut = null;
            img_cut = img.Copy();
            img_cut.ROI = rect;
            Image<Bgr, byte> temp = img_cut.Copy();

            //#region Find triangles and rectangles
            //double cannyThresholdLinking = 120.0;
            //double cannyThreshold = 180.0;
            //Image<Gray, byte> imgOutput = temp.Convert<Gray, byte>().ThresholdBinary(new Gray(150), new Gray(255));
            //UMat cannyEdges = new UMat();
            //CvInvoke.Canny(imgOutput, cannyEdges, cannyThreshold, cannyThresholdLinking);
            //List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle

            //using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {
            //    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            //    int count = contours.Size;
            //    for (int i = 0; i < count; i++) {
            //        using (VectorOfPoint contour = contours[i])
            //        using (VectorOfPoint approxContour = new VectorOfPoint()) {
            //            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.1, true);
            //            if (CvInvoke.ContourArea(approxContour, false) > 50) //only consider contours with area greater than 250
            //            {
            //                if (approxContour.Size == 4) //The contour has 4 vertices.
            //                {
            //                    #region determine if all the angles in the contour are within [80, 100] degree
            //                    bool isRectangle = true;
            //                    Point[] pts = approxContour.ToArray();
            //                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

            //                    for (int j = 0; j < edges.Length; j++) {
            //                        double angle = Math.Abs(
            //                           edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
            //                        if (angle < 70 || angle > 100) {
            //                            isRectangle = false;
            //                            break;
            //                        }
            //                    }
            //                    #endregion

            //                    if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
            //                }
            //            }
            //        }
            //    }
            //}
            //#endregion

            //#region draw triangles and rectangles
            ////Mat triangleRectangleImage = new Mat(img.Size, DepthType.Cv8U, 3);
            ////triangleRectangleImage.SetTo(new MCvScalar(0));
            ////foreach (RotatedRect box in boxList) {
            ////    CvInvoke.Polylines(triangleRectangleImage, Array.ConvertAll(box.GetVertices(), Point.Round), true, new Bgr(Color.DarkOrange).MCvScalar, 2);
            ////}
            ////pictureBox1.Image = triangleRectangleImage.Bitmap;
            ////MessageBox.Show("ss");
            ////return;
            //#endregion


            //BarcodeReader reader = new BarcodeReader();
            //var result = reader.Decode(temp.Bitmap);
            //foreach (RotatedRect box in boxList) {
            //    var rRect = new RotatedRect(new PointF(100, 100), new SizeF(100, 50), 30);
            //    DrawRect(mat, rRect, new MCvScalar(255, 0, 0));
            //    var brect = CvInvoke.BoundingRectangle(new VectorOfPointF(rRect.GetVertices()));
            //    CvInvoke.Rectangle(mat, brect, new MCvScalar(0, 255, 0), 1, LineType.EightConnected, 0);
            //    Image<Bgr, byte> img_cut_2 = null;
            //    img_cut_2 = temp.Copy();
            //    img_cut_2.ROI = box;
            //    Image<Bgr, byte> temp_2 = img_cut_2.Copy();
            //    result = reader.Decode(temp_2.Bitmap);
            //    Bgr a = new Bgr();
            //    for (int i = 1; i <= 10; i++) {
            //        result = reader.Decode(temp_2.Bitmap);
            //        if (result != null) break;
            //        temp_2 = temp_2.Rotate(i * 8, a);
            //    }
            //}


            //BarcodeReader reader = new BarcodeReader();
            //var result = reader.Decode(temp.Bitmap);
            //Bgr a = new Bgr();
            //for (int i = 1; i <= 10; i++) {
            //    result = reader.Decode(temp.Bitmap);
            //    if (result != null) break;
            //    temp = temp.Rotate(i * 8, a);
            //}

            //if (result != null) {
            //    //CvInvoke.PutText(img, result.Text, new Point(20, 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            //    if (flag_set_camera == false && debug == false) {
            //        File.WriteAllText("test_head_" + head + "_result.txt", result + "\r\nPASS");
            //        this.Close();
            //    }
            //    flag_result = true;
            //    result_blackup = result.ToString();
            //} else {
            //    //CvInvoke.PutText(img, "not read", new Point(20, 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            //    //if (flag_set_camera == false) process_function();
            //    flag_result = false;
            //}
            if (flag_set_camera == true) {
                pictureBox1.Image = img.Bitmap;
            } else {
                temp = temp.Resize(capture.Width, capture.Height, Inter.Linear);
                temp = temp.Rotate(180, new Bgr());
                pictureBox1.Image = temp.Bitmap;
            }
        }

        private void fail_2d() {
            if (debug == true) { timeout.Reset(); return; }
            File.WriteAllText("test_head_" + head + "_result.txt", "Unreadable\r\nFAIL");
            this.Close();
        }

        private void fail_matching() {
            if (debug == true) { timeout.Reset(); return; }
            File.WriteAllText("test_head_" + head + "_result.txt", "Not found\r\nFAIL");
            this.Close();
        }

        private void fail_check_led() {
            if (debug == true) { timeout.Reset(); return; }
            File.WriteAllText("test_head_" + head + "_result.txt", "time over\r\nFAIL");
            this.Close();
        }

        private void compar_image(object sender, EventArgs e) {
            if (timeout.ElapsedMilliseconds >= time_out) { fail_matching(); if (debug == false) return; }
            if (IsMouseDown == true) return;
            if (capture != null && capture.Ptr != IntPtr.Zero) {
                Mat frame = capture.QueryFrame();
                try {
                    img = frame.ToImage<Bgr, Byte>();
                } catch (Exception) {
                    MessageBox.Show("ไม่สามารถเปิดกล้องได้");
                    Application.Exit();
                    return;
                }
            }
            Graphics g = Graphics.FromImage(img.Bitmap);
            g.DrawRectangle(Pens.Red, rect);
            Image<Bgr, byte> img_cut = null;
            img_cut = img.Copy();
            img_cut.ROI = rect;

            Image<Bgr, Byte> img1 = img_cut.Copy();

            long matchTime;
            try {
                using (Mat modelImage = CvInvoke.Imread("../../config/test_head_" + head + "_" + steptest + ".png", ImreadModes.Grayscale))
                using (Mat observedImage = img1.Mat) {
                    Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
                    CvInvoke.PutText(img, DrawMatches.get_num_object().ToString(), new Point(20, 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                    pictureBox1.Image = img.Bitmap;
                }
                if (DrawMatches.get_num_object() == false) {
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt")) - 35);
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt")));
                    flag_result = false;
                } else {
                    if (flag_set_camera == false && debug == false) {
                        if (Convert.ToBoolean(File.ReadAllText("../../config/test_head_" + head + "_debug.txt"))) debug = true;
                        else {
                            if (!timeout_show.IsRunning) timeout_show.Restart();
                            if (timeout_show.ElapsedMilliseconds < 50) return;
                        }
                        timeout_show.Stop();
                        int d = 0;
                        string c = "";
                        try {
                            d = Convert.ToInt32(steptest.Substring(steptest.Length - 1, 1));
                            d++;
                            c = steptest.Substring(0, steptest.Length - 1) + d;
                        } catch (Exception) { }
                        if (File.Exists("../../config/test_head_" + head + "_" + steptest + "2.png") ||
                            File.Exists("../../config/test_head_" + head + "_" + c + ".png")) {
                            if (c == "") steptest = steptest + "2";
                            else steptest = c;
                            try {
                                rect.X = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_x_" + steptest + ".txt"));
                                rect.Y = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_y_" + steptest + ".txt"));
                                rect.Width = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_width_" + steptest + ".txt"));
                                rect.Height = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_rect_height_" + steptest + ".txt"));
                            } catch (Exception) { }
                            this.Text = steptest;
                            return;
                        }
                        if (steptest.Contains("image_compar")) {
                            File.WriteAllText("test_head_" + head + "_result.txt", "image detected\r\nNEXT");
                            wait_next();
                            return;
                        } else File.WriteAllText("test_head_" + head + "_result.txt", "image detected\r\nPASS");
                        this.Close();
                    }
                    flag_result = true;
                    result_blackup = "image detected";
                }
            } catch (Exception) { pictureBox1.Image = img.Bitmap; }
        }
        private void wait_next() {
            while (true) {
                try {
                    string h = File.ReadAllText("../../config/test_head_" + head + "_compar_image_next_tric.txt");
                } catch (Exception) { DelaymS(100); continue; }
                File.Delete("../../config/test_head_" + head + "_compar_image_next_tric.txt");
                break;
            }
            try { steptest = File.ReadAllText("../../config/test_head_" + head + "_steptest.txt"); } catch (Exception) { }
            setup();
        }

        private void check_led(object sender, EventArgs e) {
            if (timeout.ElapsedMilliseconds >= time_out) { fail_check_led(); if (debug == false) return; }
            if (IsMouseDown == true) return;
            if (capture != null && capture.Ptr != IntPtr.Zero) {
                Mat frame;
                if (flag_hsv_test == false) frame = capture.QueryFrame();
                else frame = new Mat("../../config/hsv_test.png");
                try {
                    img = frame.ToImage<Bgr, Byte>();
                    img_hsv = frame.ToImage<Hsv, Byte>();
                } catch (Exception) {
                    MessageBox.Show("ไม่สามารถเปิดกล้องได้");
                    Application.Exit();
                    return;
                }
            }
            Graphics g = Graphics.FromImage(img.Bitmap);
            g.DrawRectangle(Pens.Red, rect);
            Image<Bgr, byte> img_cut = null;
            Image<Hsv, byte> img_cut2 = null;
            Image<Bgr, byte> img1 = null;
            Image<Hsv, byte> img2 = null;
            int redpixels = 0;
            if (flag_hsv == false) {
                img_cut = img.Copy();
                img_cut.ROI = rect;
                img1 = img_cut.Copy();
                try { redpixels = img1.InRange(bgr_low, bgr_high).CountNonzero()[0]; } catch (Exception) { }
            } else {
                img_cut2 = img_hsv.Copy();
                img_cut2.ROI = rect;
                img2 = img_cut2.Copy();
                try { redpixels = img2.InRange(hsv_low, hsv_high).CountNonzero()[0]; } catch (Exception) { }
            }
            bool mask = false;
            if (redpixels >= hsv_mask) {
                timeout.Restart();
                if (stopwatch_hsv_timeout.ElapsedMilliseconds >= hsv_timeout) mask = true;
            } else {
                stopwatch_hsv_timeout.Restart();
                mask = false;
            }
            CvInvoke.PutText(img, redpixels.ToString(), new Point(20, 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(img, mask.ToString(), new Point(20, 60), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(img, stopwatch_hsv_timeout.ElapsedMilliseconds.ToString(), new Point(pictureBox1.Size.Width - 100, 30), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            pictureBox1.Image = img.Bitmap;
            if (mask == true) {
                if (flag_set_camera == false && debug == false) {
                    DelaymS(50);
                    File.WriteAllText("test_head_" + head + "_result.txt", "Color detected\r\nPASS");
                    this.Close();
                }
                flag_result = true;
                result_blackup = "Color detected";
            } else {
                flag_result = false;
            }
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt")) - 35);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Convert.ToDouble(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt")));
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (flag_result == true) {
                if (steptest.Contains("image_compar")) File.WriteAllText("test_head_" + head + "_result.txt", result_blackup + "\r\nNEXT");
                else File.WriteAllText("test_head_" + head + "_result.txt", result_blackup + "\r\nPASS");
            } else {
                File.WriteAllText("test_head_" + head + "_result.txt", "Unreadable\r\nFAIL");
            }
            this.Close();
        }

        private int contrast_int;
        private int contrast_int_min;
        private int contrast_int_max;
        private int brightness_int;
        private int brightness_int_min;
        private int brightness_int_max;
        private int focus_int;
        private int focus_int_min;
        private int focus_int_max;
        private int step_process;
        private int scale = 40;
        private bool flag_process_intro = false;
        private void process_function() {
            if (flag_process_intro == false) {
                try {
                    contrast_int = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_contrast_" + steptest + ".txt"));
                    brightness_int = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_brightness_" + steptest + ".txt"));
                    focus_int = Convert.ToInt32(File.ReadAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt"));
                } catch (Exception) {
                    contrast_int = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast);
                    brightness_int = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness);
                    focus_int = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus);
                }
                contrast_int_min = contrast_int - scale;
                contrast_int_max = contrast_int + scale;
                brightness_int_min = brightness_int - scale;
                brightness_int_max = brightness_int + scale;
                focus_int_min = focus_int - scale;
                focus_int_max = focus_int + scale;
                flag_process_intro = true;
                step_process = 0;
            }
            switch (step_process) {
                case 0:
                    focus_int -= 2;
                    if (focus_int <= focus_int_min) { focus_int += scale; step_process++; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, focus_int);
                    break;
                case 1:
                    focus_int += 2;
                    if (focus_int >= focus_int_max) { focus_int -= scale; step_process++; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, focus_int);
                    break;
                case 2:
                    contrast_int--;
                    if (contrast_int <= contrast_int_min) { contrast_int += scale; step_process++; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, contrast_int);
                    break;
                case 3:
                    contrast_int++;
                    if (contrast_int >= contrast_int_max) { contrast_int -= scale; step_process++; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, contrast_int);
                    break;
                case 4:
                    brightness_int--;
                    if (brightness_int <= brightness_int_min) { brightness_int += scale; step_process++; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, brightness_int);
                    break;
                case 5:
                    brightness_int++;
                    if (brightness_int >= brightness_int_max) { brightness_int -= scale; step_process = 0; }
                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, brightness_int);
                    break;
            }
        }

        HScrollBar h_zoom;
        Label l_zoom;
        Label s_zoom;
        HScrollBar h_pan;
        Label l_pan;
        Label s_pan;
        HScrollBar h_tilt;
        Label l_tilt;
        Label s_tilt;
        HScrollBar h_contrast;
        Label l_contrast;
        Label s_contrast;
        HScrollBar h_brightness;
        Label l_brightness;
        Label s_brightness;
        HScrollBar h_focus;
        Label l_focus;
        Label s_focus;
        HScrollBar h_process;
        Label l_process;
        Label s_process;
        Button b_process;
        Label l_bgr;
        TextBox t_bgr;
        Label l_hsv;
        TextBox t_hsv;
        Button b_hsv;
        TextBox t_hsv_mask;
        Label l_mask;
        Label l_hsv_test;
        Label l_timeout;
        TextBox t_timeout;
        Button b_example;
        HScrollBar h_exposure;
        Label l_exposure;
        Label s_exposure;
        private void setCameraToolStripMenuItem_Click(object sender, EventArgs e) {
            if (flag_add_step) return;
            flag_set_camera = true;
            Form f1 = new Form();
            f1.FormClosed += F1_FormClosed;
            f1.Size = new Size(400, 430);
            h_zoom = new HScrollBar();
            l_zoom = new Label();
            s_zoom = new Label();
            h_pan = new HScrollBar();
            l_pan = new Label();
            s_pan = new Label();
            h_tilt = new HScrollBar();
            l_tilt = new Label();
            s_tilt = new Label();
            h_contrast = new HScrollBar();
            l_contrast = new Label();
            s_contrast = new Label();
            h_brightness = new HScrollBar();
            l_brightness = new Label();
            s_brightness = new Label();
            h_focus = new HScrollBar();
            l_focus = new Label();
            s_focus = new Label();
            h_process = new HScrollBar();
            l_process = new Label();
            s_process = new Label();
            b_process = new Button();
            l_bgr = new Label();
            t_bgr = new TextBox();
            l_hsv = new Label();
            t_hsv = new TextBox();
            b_hsv = new Button();
            t_hsv_mask = new TextBox();
            l_mask = new Label();
            l_hsv_test = new Label();
            l_timeout = new Label();
            t_timeout = new TextBox();
            b_example = new Button();
            h_exposure = new HScrollBar();
            l_exposure = new Label();
            s_exposure = new Label();

            l_zoom.Text = "zoom";
            l_zoom.Size = new Size(300, 15);
            l_zoom.Location = new Point(1, 1);
            f1.Controls.Add(l_zoom);
            h_zoom.Scroll += H_zoom_Scroll;
            h_zoom.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_zoom.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_zoom_min_read2d.txt")); } catch (Exception) { h_zoom.Minimum = -999; }
                try { h_zoom.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_zoom_max_read2d.txt")); } catch (Exception) { h_zoom.Maximum = 999; }
            } else {
                try { h_zoom.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_zoom_min.txt")); } catch (Exception) { h_zoom.Minimum = -999; }
                try { h_zoom.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_zoom_max.txt")); } catch (Exception) { h_zoom.Maximum = 999; }
            }
            try { h_zoom.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom); } catch (Exception) { }
            h_zoom.Size = new Size(300, h_zoom.Height);
            h_zoom.Location = new Point(1, 15);
            f1.Controls.Add(h_zoom);
            s_zoom.Text = h_zoom.Value.ToString();
            s_zoom.Size = new Size(300, 15);
            s_zoom.Location = new Point(h_zoom.Size.Width + 5, h_zoom.Location.Y + 2);
            f1.Controls.Add(s_zoom);

            l_pan.Text = "pan";
            l_pan.Size = new Size(300, 15);
            l_pan.Location = new Point(1, 40);
            f1.Controls.Add(l_pan);
            h_pan.Scroll += H_pan_Scroll;
            h_pan.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_pan.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_pan_min_read2d.txt")); } catch (Exception) { h_pan.Minimum = -999; }
                try { h_pan.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_pan_max_read2d.txt")); } catch (Exception) { h_pan.Maximum = 999; }
            } else {
                try { h_pan.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_pan_min.txt")); } catch (Exception) { h_pan.Minimum = -999; }
                try { h_pan.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_pan_max.txt")); } catch (Exception) { h_pan.Maximum = 999; }
            }
            try { h_pan.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Pan); } catch (Exception) { }
            h_pan.Size = new Size(300, h_pan.Height);
            h_pan.Location = new Point(1, 55);
            f1.Controls.Add(h_pan);
            s_pan.Text = h_pan.Value.ToString();
            s_pan.Size = new Size(300, 15);
            s_pan.Location = new Point(h_pan.Size.Width + 5, h_pan.Location.Y + 2);
            f1.Controls.Add(s_pan);

            l_tilt.Text = "tilt";
            l_tilt.Size = new Size(300, 15);
            l_tilt.Location = new Point(1, 80);
            f1.Controls.Add(l_tilt);
            h_tilt.Scroll += H_tilt_Scroll;
            h_tilt.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_tilt.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_tilt_min_read2d.txt")); } catch (Exception) { h_tilt.Minimum = -999; }
                try { h_tilt.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_tilt_max_read2d.txt")); } catch (Exception) { h_tilt.Maximum = 999; }
            } else {
                try { h_tilt.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_tilt_min.txt")); } catch (Exception) { h_tilt.Minimum = -999; }
                try { h_tilt.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_tilt_max.txt")); } catch (Exception) { h_tilt.Maximum = 999; }
            }
            try { h_tilt.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Tilt); } catch (Exception) { }
            h_tilt.Size = new Size(300, h_tilt.Height);
            h_tilt.Location = new Point(1, 95);
            f1.Controls.Add(h_tilt);
            s_tilt.Text = h_tilt.Value.ToString();
            s_tilt.Size = new Size(300, 15);
            s_tilt.Location = new Point(h_tilt.Size.Width + 5, h_tilt.Location.Y + 2);
            f1.Controls.Add(s_tilt);

            l_contrast.Text = "contrast";
            l_contrast.Size = new Size(300, 15);
            l_contrast.Location = new Point(1, 120);
            f1.Controls.Add(l_contrast);
            h_contrast.Scroll += H_contrast_Scroll;
            h_contrast.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_contrast.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_contrast_min_read2d.txt")); } catch (Exception) { h_contrast.Minimum = -999; }
                try { h_contrast.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_contrast_max_read2d.txt")); } catch (Exception) { h_contrast.Maximum = 999; }
            } else {
                try { h_contrast.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_contrast_min.txt")); } catch (Exception) { h_contrast.Minimum = -999; }
                try { h_contrast.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_contrast_max.txt")); } catch (Exception) { h_contrast.Maximum = 999; }
            }
            try { h_contrast.Value = ((int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast)); } catch (Exception) { }
            h_contrast.Size = new Size(300, h_contrast.Height);
            h_contrast.Location = new Point(1, 135);
            f1.Controls.Add(h_contrast);
            s_contrast.Text = h_contrast.Value.ToString();
            s_contrast.Size = new Size(300, 15);
            s_contrast.Location = new Point(h_contrast.Size.Width + 5, h_contrast.Location.Y + 2);
            f1.Controls.Add(s_contrast);

            l_brightness.Text = "brightness";
            l_brightness.Size = new Size(300, 15);
            l_brightness.Location = new Point(1, 160);
            f1.Controls.Add(l_brightness);
            h_brightness.Scroll += H_brightness_Scroll;
            h_brightness.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_brightness.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_brightness_min_read2d.txt")); } catch (Exception) { h_brightness.Minimum = -999; }
                try { h_brightness.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_brightness_max_read2d.txt")); } catch (Exception) { h_brightness.Maximum = 999; }
            } else {
                try { h_brightness.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_brightness_min.txt")); } catch (Exception) { h_brightness.Minimum = -999; }
                try { h_brightness.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_brightness_max.txt")); } catch (Exception) { h_brightness.Maximum = 999; }
            }
            try { h_brightness.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness); } catch (Exception) { }
            h_brightness.Size = new Size(300, h_brightness.Height);
            h_brightness.Location = new Point(1, 175);
            f1.Controls.Add(h_brightness);
            s_brightness.Text = h_brightness.Value.ToString();
            s_brightness.Size = new Size(300, 15);
            s_brightness.Location = new Point(h_brightness.Size.Width + 5, h_brightness.Location.Y + 2);
            f1.Controls.Add(s_brightness);

            l_focus.Text = "focus";
            l_focus.Size = new Size(300, 15);
            l_focus.Location = new Point(1, 200);
            f1.Controls.Add(l_focus);
            h_focus.Scroll += H_focus_Scroll;
            h_focus.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_focus.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_focus_min_read2d.txt")); } catch (Exception) { h_focus.Minimum = -999; }
                try { h_focus.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_focus_max_read2d.txt")); } catch (Exception) { h_focus.Maximum = 999; }
            } else {
                try { h_focus.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_focus_min.txt")); } catch (Exception) { h_focus.Minimum = -999; }
                try { h_focus.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_focus_max.txt")); } catch (Exception) { h_focus.Maximum = 999; }
            }
            try { h_focus.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus); } catch (Exception) { }
            h_focus.Size = new Size(300, h_focus.Height);
            h_focus.Location = new Point(1, 215);
            f1.Controls.Add(h_focus);
            s_focus.Text = h_focus.Value.ToString();
            s_focus.Size = new Size(30, 15);
            s_focus.Location = new Point(h_focus.Size.Width + 5, h_focus.Location.Y + 2);
            f1.Controls.Add(s_focus);
            Button b_focus = new Button();
            b_focus.Click += B_focus_Click;
            b_focus.Text = "auto";
            b_focus.Size = new Size(40, 20);
            b_focus.Location = new Point(h_focus.Size.Width + 40, h_focus.Location.Y);
            f1.Controls.Add(b_focus);

            l_process.Text = "process";
            l_process.Size = new Size(300, 15);
            l_process.Location = new Point(1, 240);
            f1.Controls.Add(l_process);
            h_process.Scroll += H_process_Scroll;
            h_process.LargeChange = 1;
            h_process.Minimum = 0;
            h_process.Maximum = 255;
            h_process.Value = process_value;
            h_process.Size = new Size(300, h_process.Height);
            h_process.Location = new Point(1, 255);
            f1.Controls.Add(h_process);
            s_process.Text = h_process.Value.ToString();
            s_process.Size = new Size(30, 15);
            s_process.Location = new Point(h_process.Size.Width + 5, h_process.Location.Y + 2);
            f1.Controls.Add(s_process);
            b_process.Click += B_process_Click;
            b_process.Text = flag_process.ToString();
            b_process.Size = new Size(40, 20);
            b_process.Location = new Point(h_process.Size.Width + 40, h_process.Location.Y);
            f1.Controls.Add(b_process);

            l_bgr.Text = "bgr: \"BlurLow BlueHigh GreenLow GreenHigh RedLow RedHigh\"";
            l_bgr.Size = new Size(400, 15);
            l_bgr.Location = new Point(1, 280);
            f1.Controls.Add(l_bgr);
            t_bgr.Text = bgr_low.Blue.ToString() + " " + bgr_high.Blue.ToString() + " " +
                         bgr_low.Green.ToString() + " " + bgr_high.Green.ToString() + " " +
                         bgr_low.Red.ToString() + " " + bgr_high.Red.ToString();
            t_bgr.Size = new Size(180, 20);
            t_bgr.Location = new Point(1, l_bgr.Location.Y + 15);
            t_bgr.KeyDown += T_bgr_KeyDown;
            f1.Controls.Add(t_bgr);
            l_mask.Text = "mask :";
            l_mask.Size = new Size(40, 15);
            l_mask.Location = new Point(t_bgr.Size.Width + 85, t_bgr.Location.Y + 2);
            f1.Controls.Add(l_mask);
            t_hsv_mask.Text = hsv_mask.ToString();
            t_hsv_mask.Size = new Size(75, 20);
            t_hsv_mask.Location = new Point(t_bgr.Size.Width + 125, t_bgr.Location.Y);
            t_hsv_mask.KeyDown += T_hsv_mask_KeyDown;
            f1.Controls.Add(t_hsv_mask);
            b_example.Click += B_example_Click;
            b_example.Text = "example";
            b_example.Size = new Size(60, 20);
            b_example.Location = new Point(t_bgr.Size.Width + 10, t_bgr.Location.Y);
            f1.Controls.Add(b_example);

            l_hsv.Text = "hsv: \"HueLow HueHigh SatuationLow SatuationHigh ValueLow ValueHigh\"";
            l_hsv.Size = new Size(400, 15);
            l_hsv.Location = new Point(1, 320);
            f1.Controls.Add(l_hsv);
            t_hsv.Text = hsv_low.Hue.ToString() + " " + hsv_high.Hue.ToString() + " " +
                         hsv_low.Satuation.ToString() + " " + hsv_high.Satuation.ToString() + " " +
                         hsv_low.Value.ToString() + " " + hsv_high.Value.ToString();
            t_hsv.Size = new Size(180, 20);
            t_hsv.Location = new Point(1, l_hsv.Location.Y + 15);
            t_hsv.KeyDown += T_hsv_KeyDown;
            f1.Controls.Add(t_hsv);
            l_timeout.Text = "timeout :";
            l_timeout.Size = new Size(47, 15);
            l_timeout.Location = new Point(t_hsv.Size.Width + 10, t_hsv.Location.Y + 2);
            f1.Controls.Add(l_timeout);
            t_timeout.Text = hsv_timeout.ToString();
            t_timeout.Size = new Size(60, 20);
            t_timeout.Location = new Point(t_hsv.Size.Width + 57, t_hsv.Location.Y);
            t_timeout.KeyDown += T_timeout_KeyDown;
            f1.Controls.Add(t_timeout);
            l_hsv_test.Text = "ms";
            l_hsv_test.Size = new Size(30, 15);
            l_hsv_test.Location = new Point(t_hsv.Size.Width + 120, t_hsv.Location.Y + 2);
            f1.Controls.Add(l_hsv_test);
            b_hsv.Click += B_hsv_Click;
            b_hsv.Text = flag_hsv_test.ToString();
            b_hsv.Size = new Size(40, 20);
            b_hsv.Location = new Point(t_hsv.Size.Width + 160, t_hsv.Location.Y);
            f1.Controls.Add(b_hsv);

            l_exposure.Text = "exposure";
            l_exposure.Size = new Size(300, 15);
            l_exposure.Location = new Point(1, 360);
            f1.Controls.Add(l_exposure);
            h_exposure.Scroll += H_exposure_Scroll;
            h_exposure.LargeChange = 1;
            if (steptest_camera_read2d_flag == true) {
                try { h_exposure.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_exposure_min_read2d.txt")); } catch (Exception) { h_exposure.Minimum = -999; }
                try { h_exposure.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_exposure_max_read2d.txt")); } catch (Exception) { h_exposure.Maximum = 999; }
            } else {
                try { h_exposure.Minimum = Convert.ToInt32(File.ReadAllText("../../config/cam_exposure_min.txt")); } catch (Exception) { h_exposure.Minimum = -999; }
                try { h_exposure.Maximum = Convert.ToInt32(File.ReadAllText("../../config/cam_exposure_max.txt")); } catch (Exception) { h_exposure.Maximum = 999; }
            }
            try { h_exposure.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure); } catch (Exception) { }
            h_exposure.Size = new Size(300, h_exposure.Height);
            h_exposure.Location = new Point(1, 375);
            f1.Controls.Add(h_exposure);
            s_exposure.Text = h_exposure.Value.ToString();
            s_exposure.Size = new Size(300, 15);
            s_exposure.Location = new Point(h_exposure.Size.Width + 5, h_exposure.Location.Y + 2);
            f1.Controls.Add(s_exposure);

            f1.Show();
        }

        private void configLeToolStripMenuItem_Click(object sender, EventArgs e) {
            string str_read2d = "";
            int length = 100;
            if (steptest_camera_read2d_flag == true) str_read2d = "_read2d";
            double min = 0, max = 0;
            bool next = false; // Zoom
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_zoom_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_zoom_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // exposure
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_exposure_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_exposure_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // pan
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Pan, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Pan)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_pan_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_pan_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // tilt
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Tilt, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Tilt)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_tilt_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_tilt_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // contrast
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_contrast_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_contrast_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // brightness
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_brightness_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_brightness_min" + str_read2d + ".txt", min.ToString());
            min = 0;
            max = 0;
            next = false; // focus
            for (double gh = -length; gh < length; gh += 1) {
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, gh);
                if (gh != capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus)) { if (!next) continue; break; }
                if (!next) { min = gh; max = gh; next = true; }
                max = gh;
            }
            File.WriteAllText("../../config/cam_focus_max" + str_read2d + ".txt", max.ToString());
            File.WriteAllText("../../config/cam_focus_min" + str_read2d + ".txt", min.ToString());
        }

        private void B_example_Click(object sender, EventArgs e) {
            MessageBox.Show("green : bgr : 0 100 100 255 0 50" +
                            "red : hsv : 0 60 0 255 150 255");
        }

        private void T_timeout_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            string cc = t_timeout.Text;
            int aa;
            try {
                aa = Convert.ToInt32(cc);
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            hsv_timeout = aa;
        }

        private void T_hsv_mask_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            string cc = t_hsv_mask.Text;
            int aa;
            try {
                aa = Convert.ToInt32(cc);
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            hsv_mask = aa;
        }

        private void B_hsv_Click(object sender, EventArgs e) {
            if (b_hsv.Text == "True") {
                b_hsv.Text = "False";
                flag_hsv_test = false;
            } else {
                b_hsv.Text = "True";
                flag_hsv_test = true;
            }
        }

        private void T_hsv_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            string cc = t_hsv.Text;
            string[] zz;
            int[] xx = { 0, 0, 0, 0, 0, 0 };
            try {
                zz = cc.Split(' ');
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            if (zz.Length != 6) { MessageBox.Show("not formath"); return; }
            try {
                for (int i = 0; i < 6; i++) {
                    xx[i] = Convert.ToInt32(zz[i]);
                }
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            hsv_low = new Hsv(xx[0], xx[2], xx[4]);
            hsv_high = new Hsv(xx[1], xx[3], xx[5]);
            flag_hsv = true;
        }

        private void T_bgr_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            string cc = t_bgr.Text;
            string[] zz;
            int[] xx = { 0, 0, 0, 0, 0, 0 };
            try {
                zz = cc.Split(' ');
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            if (zz.Length != 6) { MessageBox.Show("not formath"); return; }
            try {
                for (int i = 0; i < 6; i++) {
                    xx[i] = Convert.ToInt32(zz[i]);
                }
            } catch (Exception) { MessageBox.Show("not formath"); return; }
            bgr_low = new Bgr(xx[0], xx[2], xx[4]);
            bgr_high = new Bgr(xx[1], xx[3], xx[5]);
            flag_hsv = false;
        }

        private void B_process_Click(object sender, EventArgs e) {
            if (b_process.Text == "True") {
                b_process.Text = "False";
                flag_process = false;
            } else {
                b_process.Text = "True";
                flag_process = true;
            }
        }

        private void H_process_Scroll(object sender, ScrollEventArgs e) {
            s_process.Text = h_process.Value.ToString();
            process_value = h_process.Value;
        }

        private void B_focus_Click(object sender, EventArgs e) {
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autofocus, 1);
            try { h_focus.Value = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus); } catch (Exception) { }
            s_focus.Text = h_focus.Value.ToString();
        }

        private void H_focus_Scroll(object sender, ScrollEventArgs e) {
            s_focus.Text = h_focus.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, h_focus.Value);
        }

        private void H_brightness_Scroll(object sender, ScrollEventArgs e) {
            s_brightness.Text = h_brightness.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, h_brightness.Value);
        }

        private void H_contrast_Scroll(object sender, ScrollEventArgs e) {
            s_contrast.Text = h_contrast.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, h_contrast.Value);
        }

        private void F1_FormClosed(object sender, FormClosedEventArgs e) {
            File.WriteAllText("../../config/test_head_" + head + "_zoom_" + steptest + ".txt", h_zoom.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_pan_" + steptest + ".txt", h_pan.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_tilt_" + steptest + ".txt", h_tilt.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_contrast_" + steptest + ".txt", h_contrast.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_brightness_" + steptest + ".txt", h_brightness.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_focus_" + steptest + ".txt", h_focus.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_exposure_" + steptest + ".txt", h_exposure.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_process_" + steptest + ".txt", h_process.Value.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_flag_process_" + steptest + ".txt", flag_process.ToString());
            if (steptest_camera_check_led_red_green == true) {
                File.WriteAllText("../../config/test_head_" + head + "_bgr_" + steptest + ".txt", bgr_low.Blue.ToString() + " " +
                                                                                                  bgr_high.Blue.ToString() + " " +
                                                                                                  bgr_low.Green.ToString() + " " +
                                                                                                  bgr_high.Green.ToString() + " " +
                                                                                                  bgr_low.Red.ToString() + " " +
                                                                                                  bgr_high.Red.ToString());
                File.WriteAllText("../../config/test_head_" + head + "_hsv_" + steptest + ".txt", hsv_low.Hue.ToString() + " " +
                                                                                                  hsv_high.Hue.ToString() + " " +
                                                                                                  hsv_low.Satuation.ToString() + " " +
                                                                                                  hsv_high.Satuation.ToString() + " " +
                                                                                                  hsv_low.Value.ToString() + " " +
                                                                                                  hsv_high.Value.ToString());
                File.WriteAllText("../../config/test_head_" + head + "_flag_hsv_" + steptest + ".txt", flag_hsv.ToString());
                File.WriteAllText("../../config/test_head_" + head + "_hsv_mask_" + steptest + ".txt", hsv_mask.ToString());
                File.WriteAllText("../../config/test_head_" + head + "_hsv_timeout_" + steptest + ".txt", hsv_timeout.ToString());
            }
            flag_set_camera = false;
            flag_process_intro = false;
        }

        private void H_tilt_Scroll(object sender, ScrollEventArgs e) {
            s_tilt.Text = h_tilt.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Tilt, h_tilt.Value);
        }

        private void H_pan_Scroll(object sender, ScrollEventArgs e) {
            s_pan.Text = h_pan.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Pan, h_pan.Value);
        }

        private void H_zoom_Scroll(object sender, ScrollEventArgs e) {
            s_zoom.Text = h_zoom.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom, h_zoom.Value);
        }

        private void H_exposure_Scroll(object sender, ScrollEventArgs e) {
            s_exposure.Text = h_exposure.Value.ToString();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, h_exposure.Value);
        }

        private static void DelaymS(int mS) {
            Stopwatch stopwatchDelaymS = new Stopwatch();
            stopwatchDelaymS.Restart();
            while (mS > stopwatchDelaymS.ElapsedMilliseconds) {
                if (!stopwatchDelaymS.IsRunning)
                    stopwatchDelaymS.Start();
                Application.DoEvents();
            }
            stopwatchDelaymS.Stop();
        }

        bool IsMouseDown = false;
        Point StartLocation;
        Point EndLcation;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (!flag_set_camera) return;
            if (e.Button != MouseButtons.Left) return;
            IsMouseDown = true;
            StartLocation = e.Location;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (IsMouseDown == true) {
                EndLcation = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (IsMouseDown != true) return;
            Image<Bgr, byte> imgInput;
            EndLcation = e.Location;
            IsMouseDown = false;
            if (steptest == "read2d") {
                if (rect != null) {
                    imgInput = img.Copy();
                    imgInput.ROI = rect;
                    Image<Bgr, byte> temp = imgInput.Copy();
                    BarcodeReader reader = new BarcodeReader();
                    var result = reader.Decode(temp.Bitmap);
                    if (result != null) MessageBox.Show(result.ToString());
                }
            } else if (steptest_camera_matching_lcd_oo_oe_eo_ee == true) {
                if (rect != null) {
                    imgInput = img.Copy();
                    imgInput.ROI = rect;
                    Image<Bgr, byte> temp = imgInput.Copy();
                    temp.Save("../../config/test_head_" + head + "_" + steptest + ".png");
                }
                rect.X -= crop;
                rect.Y -= crop;
                rect.Width = rect.Width + (crop * 2);
                rect.Height = rect.Height + (crop * 2);
                //if (rect.X < 0) rect.X = 0;
                //if (rect.Y < 0) rect.Y = 0;
                //if (rect.Width + rect.X > img.Size.Width) rect.Width = 0;
            } else if (steptest_camera_check_led_red_green == true) {
                if (rect != null) {
                    imgInput = img.Copy();
                    imgInput.ROI = rect;
                    Image<Bgr, byte> temp = imgInput.Copy();
                }
            }
            File.WriteAllText("../../config/test_head_" + head + "_rect_x_" + steptest + ".txt", rect.X.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_rect_y_" + steptest + ".txt", rect.Y.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_rect_width_" + steptest + ".txt", rect.Width.ToString());
            File.WriteAllText("../../config/test_head_" + head + "_rect_height_" + steptest + ".txt", rect.Height.ToString());
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            if (rect != null && IsMouseDown == true) {
                e.Graphics.DrawRectangle(Pens.Red, GetRectangle());
            }
        }

        private Rectangle GetRectangle() {
            rect.X = Math.Min(StartLocation.X, EndLcation.X);
            rect.Y = Math.Min(StartLocation.Y, EndLcation.Y);
            rect.Width = Math.Abs(StartLocation.X - EndLcation.X);
            rect.Height = Math.Abs(StartLocation.Y - EndLcation.Y);
            return rect;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            //capture.Dispose();
        }

        private void setDebugToolStripMenuItem_Click(object sender, EventArgs e) {
            debug = false;
        }

        private void setPortToolStripMenuItem_Click(object sender, EventArgs e) {
            File.WriteAllText("set_port.txt", "set port");
            capture.Dispose();
            Application.Restart();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            capture.Dispose();
        }

        private void addStepComparToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!steptest_camera_matching_lcd_oo_oe_eo_ee) return;
            File.WriteAllText("add_step.txt", "True");
            if (!flag_add_step) steptest = steptest + "2";
            else {
                int v = Convert.ToInt32(steptest.Substring(steptest.Length - 1, 1));
                v++;
                steptest = steptest.Substring(0, steptest.Length - 1) + v.ToString();
            }
            File.WriteAllText("../../config/test_head_" + head + "_steptest.txt", steptest);
            Application.Restart();
        }
    }
}
