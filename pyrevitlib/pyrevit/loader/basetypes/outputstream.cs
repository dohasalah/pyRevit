using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;

namespace PyRevitBaseClasses
{
    /// A stream to write output to...
    /// This can be passed into the python interpreter to render all output to.
    /// Only a minimal subset is actually implemented - this is all we really expect to use.
    public class ScriptOutputStream: Stream
    {
        private readonly ScriptOutput _gui;
        private string _outputBuffer;

        public ScriptOutputStream(ScriptOutput gui)
        {
            _outputBuffer = String.Empty;
            _gui = gui;
        }

        public void write(string s)
        {
            Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
        }

        public void WriteError(string error_msg)
        {
            var err_div = _gui.txtStdOut.Document.CreateElement(ExternalConfig.errordiv);
            err_div.InnerHtml = error_msg.Replace("\n", "<br/>");

            var output_err_message = err_div.OuterHtml.Replace("<", "&clt;").Replace(">", "&cgt;");
            Write(Encoding.ASCII.GetBytes(output_err_message), 0, output_err_message.Length);
        }

        /// Append the text in the buffer to gui.txtStdOut
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this)
            {
                if (_gui.IsDisposed)
                {
                    return;
                }

                if (!_gui.Visible)
                {
                    _gui.Show();
                    _gui.Focus();
                }

                var actualBuffer = new byte[count];
                Array.Copy(buffer, offset, actualBuffer, 0, count);
                var text = Encoding.UTF8.GetString(actualBuffer);

                // append output to the buffer
                _outputBuffer += text;

                if (count % 1024 != 0) {
                    // Cleanup output for html
                    if (_outputBuffer.EndsWith("\n"))
                        _outputBuffer = _outputBuffer.Remove(_outputBuffer.Length - 1);
                    _outputBuffer = _outputBuffer.Replace("<", "&lt;").Replace(">", "&gt;");
                    _outputBuffer = _outputBuffer.Replace("&clt;", "<").Replace("&cgt;", ">");
                    _outputBuffer = _outputBuffer.Replace("\n", "<br/>");
                    _outputBuffer = _outputBuffer.Replace("\t", "&emsp;&emsp;");

                    // write to output window
                    _gui.AppendText(_outputBuffer, ExternalConfig.defaultelement);

                    // reset buffer and flush state for next time
                    _outputBuffer = String.Empty;
                }
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// Read from the _inputBuffer, block until a new line has been entered...
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return 0; }
            set { }
        }
    }
}
