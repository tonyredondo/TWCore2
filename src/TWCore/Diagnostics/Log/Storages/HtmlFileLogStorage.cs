/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.Multicast;
using TWCore.Serialization;

// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Writes an html log file
    /// </summary>
    [StatusName("Html File Log")]
	public class HtmlFileLogStorage : ILogStorage
    {
        private static readonly NonBlocking.ConcurrentDictionary<string, StreamWriter> LogStreams = new NonBlocking.ConcurrentDictionary<string, StreamWriter>();
        private static readonly ConcurrentStack<StringBuilder> StringBuilderPool = new ConcurrentStack<StringBuilder>();
        private readonly Guid _discoveryServiceId;
        private StreamWriter _sWriter;
        private string _currentFileName;
        private int _numbersOfFiles;
        private volatile bool _firstWrite = true;
        private Timer _flushTimer;
        private int _shouldFlush;

        #region Html format
        private const string Html = @"
<html>
<head>
    <title>Log File</title>
	<script src='https://code.jquery.com/jquery-3.2.1.slim.min.js' integrity='sha256-k2WSCIexGzOj3Euiig+TlR8gA0EmPjuc79OEeY5L45g=' crossorigin='anonymous'></script>
    <style>
        html { background-color: #000; }
        body { background-color: #000 }
        .displaywrap::-webkit-scrollbar-track { border-radius: 0px; background-color: #181818; }
        .displaywrap::-webkit-scrollbar { width: 18px; height: 18px; background-color: #000; }
        .displaywrap::-webkit-scrollbar-thumb { border-radius: 5px; -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,.3); background-color: #505050; }
        .displaywrap {
            position: absolute;
            overflow: scroll;
            top: 0px; bottom: 0px; left: 0px; right: 0px;
            border: 0px solid #333; border-radius: 0px;
            background: #000; color: #fff;
            font-family: Consolas, monospace; font-size: 0.8em;
            padding: 40px 0px 20px 0px;
        }
        .menu {
            position: absolute;
            top: 0px; height: 27px; left: 0px; right: 18px;
            background: rgba(25,25,25,0.95);
            border-bottom: 1px solid #0c539d;
            padding: 5px;
            z-index: 100;
            overflow: hidden;
        }
        .switch {
            position: relative;
            display: inline-block;
            width: 45px;
            height: 28px;
            margin-right: 10px;
        }
        .text {
            position: relative;
            display: inline-block;
            height: 28px;
            width: 80px;
        }
        .text div {
            position: absolute;
            top: 0px; left: 0px; height: 28px; width: 100%;
            padding: 0px;
            color: #ccc;
            font-family: 'Trebuchet MS', 'Lucida Sans Unicode', 'Lucida Grande', 'Lucida Sans', Arial, sans-serif;
            font-size: 0.75em;
            text-align:center;
        }
        .slider {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: #ccc;
            -webkit-transition: .4s;
            transition: .4s;
        }
        .slider:before {
            position: absolute;
            content: '';
            height: 20px;
            width: 20px;
            left: 4px;
            bottom: 4px;
            background-color: white;
            -webkit-transition: .4s;
            transition: .4s;
        }
        input:checked + .slider { background-color: #0351a0; }
        input:focus + .slider { box-shadow: 0 0 1px #0351a0; }
        input:checked + .slider:before {
            -webkit-transform: translateX(16px);
            -ms-transform: translateX(16px);
            transform: translateX(16px);
        }
        .slider.round { border-radius: 34px; }
        .slider.round:before { border-radius: 50%; }
        .switch input {display:none;}
        .display { padding: 7px; }
        #dvStarts {
            position: absolute;
            background-color: #222;
            top: 45px;
            right: 25px;
            z-index: 1000;
            opacity: 0.8;
            text-align: center;
            padding: 10px;
            color: white;
            font-family: monospace;
            overflow-y: auto;
            border-radius: 5px;
            font-size: 0.86em;
        }
        #ulStarts {
            list-style-type: none;
            margin: 0px;
            padding: 5px;
            line-height: 1.5em;
        }
        #ulStarts li {
            cursor: pointer;
        }
        .liActive {
            color: #1b8aff;
            font-weight: bolder;
            font-size: 1.2em;
        }
        pre { padding:0px; margin: 0px; font-family: Consolas, monospace; }
        .Start { color: brown; font-size:1.3em; }
        .End { color: brown; font-size:1.3em; }
        .Error { color: red; }
        .Warning { color: yellow; }
        .InfoBasic { color: cyan; }
        .InfoMedium { color: cyan; }
        .InfoDetail { color: cyan; }
        .Debug { color: darkcyan; }
        .LibDebug { color: darkcyan; }
        .Stats { color: darkgreen; }
        .Verbose { color: gray; }
        .LibVerbose { color: darkgray; }
    </style>
</head>
<body>
<div id='dvStarts'>STARTS<ul id='ulStarts'></ul></div>
<div class='menu'>
    <label class='text' style='width:65px;'><div id='dvError'>Error</div></label>
    <label class='switch'><input type='checkbox' id='chkError' checked><span class='slider round'></span></label>
    <label class='text'><div id='dvWarning'>Warning</div></label>
    <label class='switch'><input type='checkbox' id='chkWarning' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoB'>Info B</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoB' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoM'>Info M</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoM' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoD'>Info D</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoD' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvDebug'>Debug</div></label>
    <label class='switch'><input type='checkbox' id='chkDebug' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvStats'>Stats</div></label>
    <label class='switch'><input type='checkbox' id='chkStats' checked><span class='slider round'></span></label>
    <label class='text'><div id='dvVerbose'>Verbose</div></label>
    <label class='switch'><input type='checkbox' id='chkVerbose' checked><span class='slider round'></span></label>
    <label class='text' style='width:55px;'><div>Empty<br/>Lines</div></label>
    <label class='switch'><input type='checkbox' id='chkEmptyLines' checked><span class='slider round'></span></label>
    <script>
        var dvContainer = {};
        var S = function(selector) {
            if (dvContainer[selector] === undefined)
                dvContainer[selector] = $(selector);
            return dvContainer[selector];
        };
        var starts = { 
            data : [],
            goTo : function(index) { S('.displaywrap').scrollTop(this.data[index][1].offset().top - S('.display').position().top) },
            goToEnd : function() {S('.displaywrap').scrollTop(S('.display').height()); }
        };
        $(function() {
            S('#dvError').html('Error<br/>' + S('.Error').length);
            S('#dvWarning').html('Warning<br/>' + S('.Warning').length);
            S('#dvInfoB').html('Info B<br/>' + S('.InfoBasic').length);
            S('#dvInfoM').html('Info M<br/>' + S('.InfoMedium').length);
            S('#dvInfoD').html('Info D<br/>' + S('.InfoDetail').length);
            S('#dvDebug').html('Debug<br/>' + (S('.Debug').length + S('.LibDebug').length));
            S('#dvStats').html('Stats<br/>' + S('.Stats').length);
            S('#dvVerbose').html('Verbose<br/>' + (S('.Verbose').length + S('.LibVerbose').length));

            if (S('.Error').length == 0) S('#chkError').parent().remove();
            if (S('.Warning').length == 0) S('#chkWarning').parent().remove();
            if (S('.InfoBasic').length == 0) S('#chkInfoB').parent().remove();
            if (S('.InfoMedium').length == 0) S('#chkInfoM').parent().remove();
            if (S('.InfoDetail').length == 0) S('#chkInfoD').parent().remove();
            if (S('.Debug').length + S('.LibDebug').length == 0) S('#chkDebug').parent().remove();
            if (S('.Stats').length == 0) S('#chkStats').parent().remove();
            if (S('.Verbose').length + S('.LibVerbose').length == 0) S('#chkVerbose').parent().remove();

            starts.goToEnd();

            S('.Start').each(function() { 
                var self = $(this); 
                var time = self.data('time');
                time = time.substr(0, time.length - 4);
                starts.data.push([time, self]); 
                S('#ulStarts').append(S('<li onclick=\'starts.goTo(' + (starts.data.length - 1) + ')\'>' + time + '</li>'));
            });
            S('#ulStarts').append($('<li onclick=\'starts.goToEnd()\'>EOF</li>'));
            var lis = S('#ulStarts li');
            S('.displaywrap').scroll(function() { 
                var containerTop = -S('.display').position().top;
                lis.removeClass('liActive');
                for(var i = starts.data.length - 1; i >= 0; i--)
                {
                    var startTop = starts.data[i][1].position().top;
                    var top = (containerTop + startTop) - 100;
                    if (containerTop >= top) 
                    {
                        $(lis[i]).addClass('liActive');
                        return;
                    }
                }
            });
        });
        S('input[type=checkbox]').click(function() {
            Toggle('.displaywrap');
            if (this.id == 'chkError') Toggle('.Error');
            if (this.id == 'chkWarning') Toggle('.Warning');
            if (this.id == 'chkInfoB') Toggle('.InfoBasic');
            if (this.id == 'chkInfoM') Toggle('.InfoMedium');
            if (this.id == 'chkInfoD') Toggle('.InfoDetail');
            if (this.id == 'chkDebug') { Toggle('.Debug'); Toggle('.LibDebug');}
            if (this.id == 'chkStats') Toggle('.Stats');
            if (this.id == 'chkVerbose') { Toggle('.Verbose'); Toggle('.LibVerbose');}
            if (this.id == 'chkEmptyLines') Toggle('.EmptyLine');
            Toggle('.displaywrap');
        });
        function Toggle(selector) {
            var jitem = S(selector);
            jitem.each(function() { 
                var th = $(this)[0];
                th.style.display = th.style.display == 'none' ? '' : 'none';
            });
        };
    </script>
</div>
<div class='displaywrap'><div class='display'><pre>";

        private const string PreFormat = "<span class='{0}'>{1}<br/></span>";
        private const string PreFormatWTime = "<span class='{0}' data-time='{2}'>{1}<br/></span>";
        private const string PreFormatWType = "<span class='{0}' data-type='{2}'>{1}<br/></span>";
        private const string HtmlEnd = "</pre></div></div></body></html>";

        #endregion

        #region Properties
        /// <summary>
        /// File name with path
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// File creation date
        /// </summary>
        public DateTime FileDate { get; private set; }
        /// <summary>
        /// True if a new log file is created each day; otherwise, false
        /// </summary>
        public bool CreateByDay { get; }
        /// <summary>
        /// True if a new log file is created when a maximum length is reached; otherwise, false.
        /// </summary>
        public bool UseMaxLength { get; }
        /// <summary>
        /// Maximun length in bytes for a single file. Default value is 4Mb
        /// </summary>
        public long MaxLength { get; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlFileLogStorage(string fileName) :
            this(fileName, true)
        { }
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlFileLogStorage(string fileName, bool createByDay) :
            this(fileName, createByDay, false)
        { }
        /// <inheritdoc />
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlFileLogStorage(string fileName, bool createByDay, bool useMaxLength) :
            this(fileName, createByDay, useMaxLength, 4194304L)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        /// <param name="maxLength">Maximun length in bytes for a single file. Default value is 4Mb</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlFileLogStorage(string fileName, bool createByDay, bool useMaxLength, long maxLength)
        {
            fileName = Factory.ResolveLowLowPath(fileName);
            FileName = fileName;
            CreateByDay = createByDay;
            UseMaxLength = useMaxLength;
            MaxLength = maxLength;
            EnsureLogFile(fileName);
            _flushTimer = new Timer(obj =>
            {
                if (_sWriter == null) return;
                try
                {
                    if (Interlocked.CompareExchange(ref _shouldFlush, 0, 1) == 1)
                        _sWriter.Flush();
                }
                catch
                {
                    //
                }
            }, this, 1500, 1500);
            if (!string.IsNullOrWhiteSpace(fileName))
                _discoveryServiceId = DiscoveryService.RegisterService(DiscoveryService.FrameworkCategory, "LOG.HTML", "This is the Html Log base path", new SerializedObject(Path.GetDirectoryName(Path.GetFullPath(fileName))));
            Core.Status.Attach(collection =>
            {
                collection.Add("Configuration",
                    new StatusItemValueItem(nameof(FileName), FileName),
                    new StatusItemValueItem(nameof(CreateByDay), CreateByDay),
                    new StatusItemValueItem(nameof(UseMaxLength), UseMaxLength),
                    new StatusItemValueItem(nameof(MaxLength) + " (MB)", MaxLength.ToMegabytes())
                );
                collection.Add("Current FileDate", FileDate);
                collection.Add("Current FileName", _currentFileName);
            });
        }

        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~HtmlFileLogStorage()
        {
            DiscoveryService.UnregisterService(_discoveryServiceId);
            Core.Status.DeAttachObject(this);
            Dispose();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureLogFile(string fileName)
        {
            var dayHasChange = FileDate.Date != DateTime.Today;
            var maxLengthReached = false;
            if (UseMaxLength && _currentFileName.IsNotNullOrWhitespace() && File.Exists(_currentFileName))
            {
                var fileLength = new FileInfo(_currentFileName).Length;
                if (fileLength >= MaxLength)
                    maxLengthReached = true;
            }
            if (!dayHasChange && !maxLengthReached) return;
            string oldFileName;
            if (dayHasChange && CreateByDay)
            {
                _currentFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                oldFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                _numbersOfFiles = 0;
            }
            else
            {
                _currentFileName = fileName;
                oldFileName = fileName;
            }
            if (maxLengthReached)
            {
                oldFileName = _currentFileName;
                _numbersOfFiles++;
                _currentFileName = Path.Combine(Path.GetDirectoryName(_currentFileName), Path.GetFileNameWithoutExtension(_currentFileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + "." + _numbersOfFiles + Path.GetExtension(_currentFileName));
            }

            #region Remove previous
            try
            {
                _sWriter?.WriteLine(HtmlEnd);
                _sWriter?.Dispose();
                _sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (LogStreams.TryRemove(oldFileName, out var oldWriter))
            {
                try
                {
                    oldWriter?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            #endregion

            #region Load Writer
            _sWriter = LogStreams.GetOrAdd(_currentFileName, fname =>
            {
                var folder = Path.GetDirectoryName(fname);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var alreadyExist = File.Exists(fname);
                var sw = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true));
                if (!alreadyExist)
                    sw.WriteLine(Html);
                return sw;
            });
            if (File.Exists(_currentFileName))
                FileDate = File.GetCreationTime(_currentFileName);
            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetExceptionDescription(SerializableException itemEx, StringBuilder sbuilder)
        {
            while (true)
            {
                sbuilder.AppendFormat("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n\r\n", itemEx.ExceptionType, System.Security.SecurityElement.Escape(itemEx.Message).Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
                if (itemEx.InnerException == null) break;
                itemEx = itemEx.InnerException;

            }
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(ILogItem item)
        {
            EnsureLogFile(FileName);
            if (_sWriter == null) return;
            if (!StringBuilderPool.TryPop(out var strBuffer))
                strBuffer = new StringBuilder();
            var time = item.Timestamp.GetTimeSpanFormat();
            var format = PreFormat;
            if (_firstWrite)
            {
                strBuffer.AppendFormat(PreFormat, "EmptyLine", "<br/>");
                strBuffer.AppendFormat(PreFormat, "EmptyLine", "<br/>");
                strBuffer.AppendFormat(PreFormat, "EmptyLine", "<br/>");
                strBuffer.AppendFormat(PreFormat, "EmptyLine", "<br/>");
                strBuffer.AppendFormat(PreFormat, "EmptyLine", "<br/>");
                strBuffer.AppendFormat(PreFormatWTime, "Start", "&#8615; START &#8615;", time);
                _firstWrite = false;
            }
            strBuffer.Append(time);
            strBuffer.AppendFormat("{0, 11}: ", item.Level);

            if (!string.IsNullOrEmpty(item.GroupName))
                strBuffer.Append(item.GroupName + " | ");

            if (item.LineNumber > 0)
                strBuffer.AppendFormat("&lt;{0};{1:000}&gt; ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
            else if (!string.IsNullOrEmpty(item.TypeName))
            {
                strBuffer.Append("&lt;" + item.TypeName + "&gt; ");
                format = PreFormatWType;
            }

            if (!string.IsNullOrEmpty(item.Code))
                strBuffer.Append("[" + item.Code + "] ");

            strBuffer.Append(System.Security.SecurityElement.Escape(item.Message));

            if (item.Exception != null)
            {
                strBuffer.Append("\r\nExceptions:\r\n");
                GetExceptionDescription(item.Exception, strBuffer);
            }
            var buffer = strBuffer.ToString();
            strBuffer.Clear();
            StringBuilderPool.Push(strBuffer);
            await _sWriter.WriteAsync(string.Format(format, item.Level, buffer, item.TypeName)).ConfigureAwait(false);
            Interlocked.Exchange(ref _shouldFlush, 1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteEmptyLineAsync()
        {
            await _sWriter.WriteAsync(string.Format(PreFormat, "EmptyLine", "<br/>")).ConfigureAwait(false);
            Interlocked.Exchange(ref _shouldFlush, 1);
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            try
            {
                _flushTimer?.Dispose();
                _flushTimer = null;
                _sWriter?.Write(PreFormat, "End", "&#8613; END &#8613;");
                _sWriter?.Flush();
                _sWriter?.Dispose();
                _sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (!LogStreams.TryRemove(_currentFileName, out var oldWriter)) return;
            try
            {
                oldWriter?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}
