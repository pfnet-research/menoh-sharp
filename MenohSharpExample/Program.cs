using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenohSharpExample
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            // Aliases to onnx's node input and output tensor name
            // Please use `/tool/onnx_viewer`
            string conv1_1_in_name = "140326425860192";
            string fc6_out_name = "140326200777584";
            string softmax_out_name = "140326200803680";

            const int batch_size = 1;
            const int channel_num = 3;
            const int height = 224;
            const int width = 224;

            var input_image_path = "../data/Light_sussex_hen.jpg";
            var onnx_model_path = "../data/VGG16.onnx";
            var synset_words_path = "../data/synset_words.txt";

            var image = System.Drawing.Bitmap.FromFile(input_image_path) as System.Drawing.Bitmap;
            if(image == null)
            {
                throw new Exception("Invalid input image path: " + input_image_path);
            }
            
            image = CropAndResize(image, width, height);
            var image_data = RenderToNCTW(image);

            // Load ONNX model data
            var model_data = MenohSharp.ModelData.MakeModelDataFromONNX(onnx_model_path);

            // Define input profile (name, dtype, dims) and output profile (name, dtype)
            // dims of output is automatically calculated later
            var vpt_builder = new MenohSharp.VariableProfileTableBuilder();
            vpt_builder.AddInputProfile(conv1_1_in_name, MenohSharp.DType.Float,
                                    new[] { batch_size, channel_num, height, width });

            vpt_builder.AddOutputProfile(fc6_out_name, MenohSharp.DType.Float);
            vpt_builder.AddOutputProfile(softmax_out_name, MenohSharp.DType.Float);

            // Build variable_profile_table and get variable dims (if needed)
            var vpt = vpt_builder.BuildVariableProfileTable(model_data);
            var softmax_dims = vpt.GetVariableProfile(softmax_out_name).Dims;

            // Make model_builder and attach extenal memory buffer
            // Variables which are not attached external memory buffer here are attached
            // internal memory buffers which are automatically allocated
            var model_builder = new MenohSharp.ModelBuilder(vpt);

            fixed (float* p = image_data)
            {
                model_builder.AttachExternalBuffer(conv1_1_in_name, (IntPtr)p);

                // Build model
                var model = model_builder.BuildModel(model_data, "mkldnn");
                model_data.Dispose(); // you can delete model_data explicitly after model building

                // Get buffer pointer of output
                float* fc6_output_buff = (float*)(model.GetVariable(fc6_out_name).BufferHandle);
                float* softmax_output_buff = (float*)(model.GetVariable(softmax_out_name).BufferHandle);

                // Run inference
                model.Run();

                // Get output
                for (int i = 0; i < 10; ++i)
                {
                    Console.Write((*(fc6_output_buff + i)).ToString() + " ");
                }
                Console.WriteLine();

                var categories = LoadCategoryList(synset_words_path);
                var top_k = 5;
                var top_k_indices = ExtractTopKIndexList(
                  softmax_output_buff, softmax_output_buff + softmax_dims[1], top_k);
                Console.WriteLine("top " + top_k + "  categories are");
                foreach (var ki in top_k_indices)
                {
                    Console.WriteLine(ki + " " + (*(softmax_output_buff + ki)).ToString() + "  categories are" + categories[ki]);
                }
            }
        }

        static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        static System.Drawing.Bitmap CropAndResize(System.Drawing.Bitmap image, int width, int height)
        {
            var short_edge = Math.Min(image.Width, image.Height);
            var roi = new System.Drawing.Rectangle();
            roi.X = (image.Width - short_edge) / 2;
            roi.Y = (image.Height - short_edge) / 2;
            roi.Width = short_edge;
            roi.Height = short_edge;
            var cropped = image.Clone(roi, System.Drawing.Imaging.PixelFormat.DontCare);
            var resized = ResizeImage(cropped, width, height);
            return resized;
        }

        static unsafe float[] RenderToNCTW(System.Drawing.Bitmap image)
        {
            int channels = 3;

            var data = new float[channels * image.Width * image.Height];

            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    var p = image.GetPixel(x, y);

                    data[0 * (image.Height * image.Width) + y * image.Width + x] = p.B;
                    data[1 * (image.Height * image.Width) + y * image.Width + x] = p.G;
                    data[2 * (image.Height * image.Width) + y * image.Width + x] = p.R;
                }
            }

            return data;
        }

        static unsafe int[] ExtractTopKIndexList(float* first, float* last, int k)
        {
            var q = new List<Tuple<float, int>>();

            for(var i = 0; first != last; first++, i++)
            {
                q.Add(Tuple.Create(*first, i));
            }

            return q.OrderByDescending(_ => _.Item1).Take(k).Select(_ => _.Item2).ToArray();
        }

        static unsafe string[] LoadCategoryList(string synset_words_path)
        {
            return System.IO.File.ReadAllLines(synset_words_path);
        }
    }
}
