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
    };

    class Utils
    {
        public static void Check(int errorCode)
        {
            if (errorCode == 0) return;
            var ec = (ErrorCode)errorCode;
            throw new Exception(ec + " " + DLL.menoh_get_last_error_message());
        }
    }

    public class ModelData : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public void Optimize(VariableProfileTable vpt)
        {
            Utils.Check(DLL.menoh_model_data_optimize(handle, vpt.handle));
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                DLL.menoh_delete_model_data(handle);
                handle = IntPtr.Zero;
            }
        }

        public static ModelData MakeModelDataFromONNX(string onnx_model_path)
        {
            IntPtr handle = IntPtr.Zero;
            Utils.Check(DLL.menoh_make_model_data_from_onnx(onnx_model_path, ref handle));

            return new ModelData()
            {
                handle = handle,
            };
        }
    }

    public class VariableProfile
    {
        public DType DType { get; internal set; }
        public int[] Dims { get; internal set; }
    };

    public class VariableProfileTable : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public VariableProfileTable()
        {

        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                DLL.menoh_delete_variable_profile_table(handle);
                handle = IntPtr.Zero;
            }
        }

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

    public class VariableProfileTableBuilder : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public VariableProfileTableBuilder()
        {
            Utils.Check(DLL.menoh_make_variable_profile_table_builder(ref handle));
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                DLL.menoh_delete_variable_profile_table_builder(handle);
                handle = IntPtr.Zero;
            }
        }

        public void AddInputProfile(string name, DType dtype, int[] dims)
        {
            if (dims.Length == 2)
            {
                Utils.Check(DLL.menoh_variable_profile_table_builder_add_input_profile_dims_2(handle, name, dtype, dims[0], dims[1]));
            }
            else if (dims.Length == 4)
            {
                Utils.Check(DLL.menoh_variable_profile_table_builder_add_input_profile_dims_4(handle, name, dtype, dims[0], dims[1], dims[2], dims[3]));
            }
            else
            {
                throw new Exception(string.Format("menoh invalid dims size error (2 or 4 is valid): dims size of {0}  is specified {1}", name, dims.Length));
            }
        }

        public void AddOutputProfile(string name, DType dtype)
        {
            Utils.Check(DLL.menoh_variable_profile_table_builder_add_output_profile(handle, name, dtype));
        }

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

    public class ModelBuilder : IDisposable
    {
        IntPtr handle = IntPtr.Zero;
        public ModelBuilder(VariableProfileTable variable_profile_table)
        {
            Utils.Check(DLL.menoh_make_model_builder(variable_profile_table.handle, ref handle));
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                DLL.menoh_delete_model_builder(handle);
                handle = IntPtr.Zero;
            }
        }

        public void AttachExternalBuffer(string name, IntPtr buffer_handle)
        {
            Utils.Check(DLL.menoh_model_builder_attach_external_buffer(handle, name, buffer_handle));
        }

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

    public class Model : IDisposable
    {
        internal IntPtr handle = IntPtr.Zero;

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                DLL.menoh_delete_model(handle);
                handle = IntPtr.Zero;
            }
        }

        public Variable GetVariable(string name)
        {
            int dtype = 0;

            Utils.Check(DLL.menoh_model_get_variable_dtype(handle, name, ref dtype));

            int dims_size = 0;
            Utils.Check(DLL.menoh_model_get_variable_dtype(handle, name, ref dims_size));

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

        public void Run()
        {
            Utils.Check(DLL.menoh_model_run(handle));
        }
    }
}
