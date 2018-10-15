using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MenohSharp
{
    class DLL
    {
        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static string menoh_get_last_error_message();

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_make_model_data_from_onnx(string onnx_filename, ref IntPtr dst_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static ErrorCode menoh_make_model_data_from_onnx_data_on_memory(IntPtr onnx_data, int size, ref IntPtr dst_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static void menoh_delete_model_data(IntPtr model_data);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_data_optimize(IntPtr model_data, IntPtr variable_profile_table);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_make_variable_profile_table_builder(ref IntPtr dst_handle);
        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static void menoh_delete_variable_profile_table_builder(IntPtr builder);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static ErrorCode menoh_variable_profile_table_builder_add_input_profile(IntPtr builder, string name, DType dtype, int dims_size, int[] dims);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        [Obsolete("Please replace into menoh_variable_profile_table_builder_add_input_profile")]
        public extern static int menoh_variable_profile_table_builder_add_input_profile_dims_2(IntPtr builder, string name, DType dtype, int num, int size);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        [Obsolete("Please replace into menoh_variable_profile_table_builder_add_input_profile")]
        public extern static int menoh_variable_profile_table_builder_add_input_profile_dims_4(IntPtr builder, string name, DType dtype, int num, int channel, int height, int width);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static ErrorCode menoh_variable_profile_table_builder_add_output_name(IntPtr builder, string name);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        [Obsolete("Please replace into menoh_variable_profile_table_builder_add_output_name")]
        public extern static int menoh_variable_profile_table_builder_add_output_profile(IntPtr builder, string name, DType dtype);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_build_variable_profile_table(IntPtr builder, IntPtr model_data, ref IntPtr dst_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static void menoh_delete_variable_profile_table(IntPtr variable_profile_table);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_variable_profile_table_get_dtype(IntPtr variable_profile_table, string variable_name, ref int dst_dtype);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_variable_profile_table_get_dims_size(IntPtr variable_profile_table, string variable_name, ref int dst_size);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_variable_profile_table_get_dims_at(IntPtr variable_profile_table, string variable_name, int index, ref int dst_size);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_make_model_builder(IntPtr variable_profile_table, ref IntPtr dst_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static void menoh_delete_model_builder(IntPtr model_builder);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_builder_attach_external_buffer(IntPtr builder, string variable_name, IntPtr buffer_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_build_model(IntPtr builder,IntPtr model_data, string backend_name, string backend_config, ref IntPtr dst_model_handle);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static void menoh_delete_model(IntPtr model);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_get_variable_dtype(IntPtr model, string variable_name, ref int dst_dtype);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_get_variable_dims_size(IntPtr model, string variable_name, ref int dst_size);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_get_variable_dims_at(IntPtr model, string variable_name, int index, ref int dst_size);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_get_variable_buffer_handle(IntPtr model, string variable_name, ref IntPtr dst_data);

        [DllImport("menoh", CallingConvention = CallingConvention.StdCall)]
        public extern static int menoh_model_run(IntPtr model);
    }
}
