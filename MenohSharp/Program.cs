using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenohSharp
{
    public enum DType : int
    {
        Float,
    }

    enum ErrorCode : int
    {
        success,
        std_error,
        unknown_error,
        invalid_filename,
        unsupported_onnx_opset_version,
        onnx_parse_error,
        invalid_dtype,
        invalid_attribute_type,
        unsupported_operator_attribute,
        dimension_mismatch,
        variable_not_found,
        index_out_of_range,
        json_parse_error,
        invalid_backend_name,
        unsupported_operator,
        failed_to_configure_operator,
        backend_error,
        same_named_variable_already_exist,
        unsupported_input_dims,
        same_named_parameter_already_exist,
        same_named_attribute_already_exist,
        invalid_backend_config_error,
        input_not_found_error,
        output_not_found_error,
    };

    class Utils
    {
        public static void Check(int errorCode)
        {
            if (errorCode == 0) return;
            var ec = (ErrorCode)errorCode;
            throw new Exception(ec + " " + DLL.menoh_get_last_error_message());
        }

        public static void Check(ErrorCode errorCode)
        {
            if (errorCode == 0) return;
            var ec = (ErrorCode)errorCode;
            throw new Exception(ec + " " + DLL.menoh_get_last_error_message());
        }
    }

    /// <summary>
    /// model data class
    /// </summary>
    public class ModelData : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// Optimize model_data.
        /// </summary>
        /// <param name="vpt"></param>
        /// <remarks>
        /// This function modify internal state of model_data.
        /// </remarks>
        public void Optimize(VariableProfileTable vpt)
        {
            Utils.Check(DLL.menoh_model_data_optimize(handle, vpt.handle));
        }

        ~ModelData()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    DLL.menoh_delete_model_data(handle);
                    handle = IntPtr.Zero;
                }

                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Load ONNX file and make model_data
        /// </summary>
        /// <param name="onnx_model_path"></param>
        /// <returns></returns>
        public static ModelData MakeModelDataFromONNX(string onnx_model_path)
        {
            IntPtr handle = IntPtr.Zero;
            Utils.Check(DLL.menoh_make_model_data_from_onnx(onnx_model_path, ref handle));

            return new ModelData()
            {
                handle = handle,
            };
        }

        /// <summary>
        /// Load ONNX file from memory and make model_data
        /// </summary>
        /// <param name="onnx_data"></param>
        /// <returns></returns>
        public static unsafe ModelData MakeModelDataFromONNXDataOnMemory(byte[] onnx_data)
        {
            fixed (byte* p = onnx_data)
            {
                IntPtr handle = IntPtr.Zero;
                Utils.Check(DLL.menoh_make_model_data_from_onnx_data_on_memory((IntPtr)p, onnx_data.Length, ref handle));

                return new ModelData()
                {
                    handle = handle,
                };
            }
        }
    }

    public class VariableProfile
    {
        public DType DType { get; internal set; }
        public int[] Dims { get; internal set; }
    };

    /// <summary>
    /// Key value store for variable_profile
    /// </summary>
    public class VariableProfileTable : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public VariableProfileTable()
        {

        }

        ~VariableProfileTable()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    DLL.menoh_delete_variable_profile_table(handle);
                    handle = IntPtr.Zero;
                }
                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Accessor to variable profile.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VariableProfile GetVariableProfile(string name)
        {
            int dtype = 0;

            Utils.Check(DLL.menoh_variable_profile_table_get_dtype(handle, name, ref dtype));

            int dims_size = 0;
            Utils.Check(DLL.menoh_variable_profile_table_get_dims_size(handle, name, ref dims_size));

            var dims = new int[dims_size];

            for (int i = 0; i < dims_size; ++i)
            {
                int d = 0;
                Utils.Check(DLL.menoh_variable_profile_table_get_dims_at(handle, name, i, ref d));
                dims[i] = d;
            }

            var ret = new VariableProfile();
            ret.DType = (DType)dtype;
            ret.Dims = dims;

            return ret;
        }
    }

    /// <summary>
    /// The builder class to build variable_profile_table
    /// </summary>
    public class VariableProfileTableBuilder : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public VariableProfileTableBuilder()
        {
            Utils.Check(DLL.menoh_make_variable_profile_table_builder(ref handle));
        }

        ~VariableProfileTableBuilder()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    DLL.menoh_delete_variable_profile_table_builder(handle);
                    handle = IntPtr.Zero;
                }
                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Add input profile. That profile contains name, dtype and dims.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dtype"></param>
        /// <param name="dims"></param>
        public void AddInputProfile(string name, DType dtype, int[] dims)
        {
            Utils.Check(DLL.menoh_variable_profile_table_builder_add_input_profile(handle, name, dtype, dims.Length, dims));
        }

        /// <summary>
        /// Add output name
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>
        /// dims amd dtype of output are calculated automatically when calling of menoh_build_variable_profile_table.
        /// </remarks>
        public void AddOutputName(string name)
        {
            Utils.Check(DLL.menoh_variable_profile_table_builder_add_output_name(handle, name));
        }

        /// <summary>
        /// Add output profile. That profile contains name, dtype.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dtype"></param>
        [Obsolete("please use AddOutputName() instead. ")]
        public void AddOutputProfile(string name, DType dtype)
        {
            Utils.Check(DLL.menoh_variable_profile_table_builder_add_output_profile(handle, name, dtype));
        }

        /// <summary>
        /// Factory function for variable_profile_table.
        /// </summary>
        /// <param name="model_data"></param>
        /// <returns></returns>
        public VariableProfileTable BuildVariableProfileTable(ModelData model_data)
        {
            IntPtr handle = IntPtr.Zero;
            Utils.Check(DLL.menoh_build_variable_profile_table(this.handle, model_data.handle, ref handle));

            return new VariableProfileTable()
            {
                handle = handle,
            };
        }
    }

    /// <summary>
    /// The builder class to build model.
    /// </summary>
    public class ModelBuilder : IDisposable
    {
        IntPtr handle = IntPtr.Zero;
        public ModelBuilder(VariableProfileTable variable_profile_table)
        {
            Utils.Check(DLL.menoh_make_model_builder(variable_profile_table.handle, ref handle));
        }

        ~ModelBuilder()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    DLL.menoh_delete_model_builder(handle);
                    handle = IntPtr.Zero;
                }

                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Users can attach external buffers to variables.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="buffer_handle"></param>
        /// <remarks>
        /// Variables attached no external buffer are attached internal buffers allocated automatically.
        /// </remarks>
        public void AttachExternalBuffer(string name, IntPtr buffer_handle)
        {
            Utils.Check(DLL.menoh_model_builder_attach_external_buffer(handle, name, buffer_handle));
        }

        /// <summary>
        /// Factory function for model
        /// </summary>
        /// <param name="model_data"></param>
        /// <param name="backend_name"></param>
        /// <param name="backend_config"></param>
        /// <returns></returns>
        public Model BuildModel(ModelData model_data, string backend_name, string backend_config = "")
        {
            IntPtr handle = IntPtr.Zero;
            Utils.Check(DLL.menoh_build_model(this.handle, model_data.handle, backend_name, backend_config, ref handle));

            return new Model()
            {
                handle = handle,
            };
        }
    }

    public class Variable
    {
        public DType DType { get; internal set; }
        public int[] Dims { get; internal set; }
        public IntPtr BufferHandle { get; internal set; }
    };

    /// <summary>
    /// The main component to run inference.
    /// </summary>
    public class Model : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        ~Model()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (handle != IntPtr.Zero)
                {
                    DLL.menoh_delete_model(handle);
                    handle = IntPtr.Zero;
                }
                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Accsessor to internal variable.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Variable GetVariable(string name)
        {
            int dtype = 0;

            Utils.Check(DLL.menoh_model_get_variable_dtype(handle, name, ref dtype));

            int dims_size = 0;
            Utils.Check(DLL.menoh_model_get_variable_dims_size(handle, name, ref dims_size));

            var dims = new int[dims_size];

            for (int i = 0; i < dims_size; ++i)
            {
                int d = 0;
                Utils.Check(DLL.menoh_model_get_variable_dims_at(handle, name, i, ref d));
                dims[i] = d;
            }

            IntPtr buffer = IntPtr.Zero;
            Utils.Check(DLL.menoh_model_get_variable_buffer_handle(handle, name, ref buffer));

            var ret = new Variable();
            ret.DType = (DType)dtype;
            ret.Dims = dims;
            ret.BufferHandle = buffer;

            return ret;
        }

        /// <summary>
        /// Run model inference.
        /// </summary>
        public void Run()
        {
            Utils.Check(DLL.menoh_model_run(handle));
        }
    }
}
