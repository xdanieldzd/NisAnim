using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NisAnim.OpenGL
{
    public class GLShaderManager : IDisposable
    {
        static string[] uniformSetMethods =
        {
            "Uniform1", "Uniform2", "Uniform3", "Uniform4"
        };

        static string[] uniformSetMethodsMatrix =
        {
            "UniformMatrix2", "UniformMatrix2x3", "UniformMatrix2x4",
            "UniformMatrix3", "UniformMatrix3x2", "UniformMatrix3x4",
            "UniformMatrix4", "UniformMatrix4x2", "UniformMatrix4x3"
        };

        GLHelper glHelper;

        Dictionary<string, int> programCache, vertexShaderCache, fragmentShaderCache;

        bool disposed;

        protected GLShaderManager()
        {
            disposed = false;
        }

        public GLShaderManager(GLHelper glHelper)
            : this()
        {
            if ((this.glHelper = glHelper) == null) throw new GLException(string.Format("{0}: glHelper cannot be null", MethodBase.GetCurrentMethod()));

            this.programCache = new Dictionary<string, int>();
            this.vertexShaderCache = new Dictionary<string, int>();
            this.fragmentShaderCache = new Dictionary<string, int>();
        }

        ~GLShaderManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Clear();
                }

                this.disposed = true;
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<string, int> program in this.programCache.Where(x => GL.IsProgram(x.Value)))
                GL.DeleteProgram(program.Value);

            foreach (KeyValuePair<string, int> shader in this.vertexShaderCache.Where(x => GL.IsShader(x.Value)))
                GL.DeleteShader(shader.Value);

            foreach (KeyValuePair<string, int> shader in this.fragmentShaderCache.Where(x => GL.IsShader(x.Value)))
                GL.DeleteShader(shader.Value);

            this.programCache.Clear();
            this.vertexShaderCache.Clear();
            this.fragmentShaderCache.Clear();
        }

        private int GenerateShader(ShaderType shaderType, string shaderString, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (shaderString == string.Empty) throw new GLException(string.Format("{0}: shaderString cannot be empty", callerFunc));

            int shaderObject = GL.CreateShader(shaderType);

            GL.ShaderSource(shaderObject, shaderString);
            GL.CompileShader(shaderObject);

            string info;
            int statusCode;
            GL.GetShaderInfoLog(shaderObject, out info);
            GL.GetShader(shaderObject, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1)
                throw new Exception(string.Format("{0}: Error compiling shader of type '{1}'; log as follows:\n\n{2}", callerFunc, shaderType, info));

            return shaderObject;
        }

        private int GenerateProgram(int vertexObject, int fragmentObject, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (!GL.IsShader(vertexObject)) throw new GLException(string.Format("{0}: vertexObject is not a shader object", callerFunc));
            if (!GL.IsShader(fragmentObject)) throw new GLException(string.Format("{0}: fragmentObject is not a shader object", callerFunc));

            int program = GL.CreateProgram();

            GL.AttachShader(program, vertexObject);
            GL.AttachShader(program, fragmentObject);

            GL.LinkProgram(program);

            string info;
            int statusCode;
            GL.GetProgramInfoLog(program, out info);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out statusCode);

            if (statusCode != 1)
                throw new Exception(string.Format("{0}: Error linking program; log as follows:\n\n{1}", callerFunc, info));

            GL.UseProgram(program);

            return program;
        }

        private void DeleteShader(Dictionary<string, int> cache, string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (cache.ContainsKey(name) && GL.IsShader(cache[name]))
            {
                GL.DeleteShader(cache[name]);
                cache.Remove(name);
            }
            else
                throw new GLException(string.Format("{0}: Cannot remove shader '{1}'; shader not found", callerFunc, name));
        }

        private void DeleteProgram(string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (this.programCache.ContainsKey(name) && GL.IsProgram(this.programCache[name]))
            {
                GL.DeleteProgram(this.programCache[name]);
                this.programCache.Remove(name);
            }
            else
                throw new GLException(string.Format("{0}: Cannot remove program '{1}'; program not found", callerFunc, name));
        }

        private int GetShader(Dictionary<string, int> cache, string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (cache.ContainsKey(name) && GL.IsShader(cache[name]))
                return cache[name];
            else
                throw new GLException(string.Format("{0}: Cannot get shader '{1}'; shader not found", callerFunc, name));
        }

        public int GetProgram(string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (this.programCache.ContainsKey(name) && GL.IsProgram(this.programCache[name]))
                return this.programCache[name];
            else
                throw new GLException(string.Format("{0}: Cannot get program '{1}'; program not found", callerFunc, name));
        }

        public void AddVertexShader(string name, string shader)
        {
            this.vertexShaderCache.Add(name, GenerateShader(ShaderType.VertexShader, shader, MethodBase.GetCurrentMethod().ToString()));
        }

        public void RemoveVertexShader(string name)
        {
            DeleteShader(this.vertexShaderCache, name, MethodBase.GetCurrentMethod().ToString());
        }

        public int GetVertexShader(string name)
        {
            return GetShader(this.vertexShaderCache, name, MethodBase.GetCurrentMethod().ToString());
        }

        public void AddFragmentShader(string name, string shader)
        {
            this.fragmentShaderCache.Add(name, GenerateShader(ShaderType.FragmentShader, shader, MethodBase.GetCurrentMethod().ToString()));
        }

        public void RemoveFragmentShader(string name)
        {
            DeleteShader(this.fragmentShaderCache, name, MethodBase.GetCurrentMethod().ToString());
        }

        public int GetFragmentShader(string name)
        {
            return GetShader(this.fragmentShaderCache, name, MethodBase.GetCurrentMethod().ToString());
        }

        public void AddProgram(string name, int vertexObject, int fragmentObject)
        {
            this.programCache.Add(name, GenerateProgram(vertexObject, fragmentObject, MethodBase.GetCurrentMethod().ToString()));
        }

        public void RemoveProgram(string name)
        {
            DeleteProgram(name, MethodBase.GetCurrentMethod().ToString());
        }

        public void AddProgramWithShaders(string name, string vertexShader, string fragmentShader)
        {
            int vertexObject = GenerateShader(ShaderType.VertexShader, vertexShader, MethodBase.GetCurrentMethod().ToString());
            int fragmentObject = GenerateShader(ShaderType.FragmentShader, fragmentShader, MethodBase.GetCurrentMethod().ToString());

            this.vertexShaderCache.Add(name, vertexObject);
            this.fragmentShaderCache.Add(name, fragmentObject);

            this.programCache.Add(name, GenerateProgram(vertexObject, fragmentObject, MethodBase.GetCurrentMethod().ToString()));
        }

        public void RemoveProgramWithShaders(string name)
        {
            DeleteShader(this.vertexShaderCache, name, MethodBase.GetCurrentMethod().ToString());
            DeleteShader(this.fragmentShaderCache, name, MethodBase.GetCurrentMethod().ToString());
            DeleteProgram(name, MethodBase.GetCurrentMethod().ToString());
        }

        public void ActivateProgram(string name)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", MethodBase.GetCurrentMethod()));

            if (this.programCache.ContainsKey(name) && GL.IsProgram(this.programCache[name]))
                GL.UseProgram(this.programCache[name]);
            else
                throw new GLException(string.Format("{0}: Cannot activate program '{1}'; program not found", MethodBase.GetCurrentMethod(), name));
        }

        public void SetUniform(string programName, string uniformName, object data)
        {
            int program = GetProgram(programName, MethodBase.GetCurrentMethod().ToString());
            int location = GL.GetUniformLocation(program, uniformName);

            if (location == -1) throw new GLException(string.Format("{0}: Cannot get location for '{1}'; uniform not found", MethodBase.GetCurrentMethod(), uniformName));

            foreach (string methodName in uniformSetMethods)
            {
                MethodInfo methodInfo = typeof(GL).GetMethod(methodName, new Type[] { typeof(int), data.GetType() });
                if (methodInfo != null)
                {
                    methodInfo.Invoke(null, new object[] { location, data });
                    return;
                }
            }

            throw new GLException(string.Format("{0}: Cannot set uniform value; unsupported type for data '{1}'", MethodBase.GetCurrentMethod(), data.GetType()));
        }

        public void SetUniformMatrix(string programName, string uniformName, bool transpose, object data)
        {
            int program = GetProgram(programName, MethodBase.GetCurrentMethod().ToString());
            int location = GL.GetUniformLocation(program, uniformName);

            if (location == -1) throw new GLException(string.Format("{0}: Cannot get location for '{1}'; uniform not found", MethodBase.GetCurrentMethod(), programName));

            foreach (string methodName in uniformSetMethodsMatrix)
            {
                MethodInfo methodInfo = typeof(GL).GetMethod(methodName, new Type[] { typeof(int), typeof(bool), data.GetType().MakeByRefType() });
                if (methodInfo != null)
                {
                    methodInfo.Invoke(null, new object[] { location, transpose, data });
                    return;
                }
            }

            throw new GLException(string.Format("{0}: Cannot set uniform value; unsupported type for data '{1}'", MethodBase.GetCurrentMethod(), data.GetType()));
        }
    }
}
