///////////////////////////////////////////////////////////////////////
//        Copyright©2003-2022 Paul Amonson, All Rights Reserved      //
///////////////////////////////////////////////////////////////////////
#region Using Section
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
#endregion

namespace Amonson.Utils
{
	/// <summary>
	/// Class to execute a command through the shell.
	/// </summary>
	/// <remarks>This class uses many overloaded methods for the common shell commands.</remarks>
	public sealed class Shell
	{
        /// <summary>
        /// List of show window type flags.
        /// </summary>
        /// <remarks>These map 1:1 to the Win32 implementation of this enum.</remarks>
        public enum ShowWindowCommands
        {
            /// <summary>
            /// Hide the window.
            /// </summary>
            SW_HIDE             = 0,
            /// <summary>
            /// Show the window as normal.
            /// </summary>
            SW_SHOWNORMAL       = 1,
            /// <summary>
            /// Same as SW_SHOWNORMAL
            /// </summary>
            SW_NORMAL           = 1,
            /// <summary>
            /// Show windows minimized.
            /// </summary>
            SW_SHOWMINIMIZED    = 2,
            /// <summary>
            /// Show windows maximized.
            /// </summary>
            SW_SHOWMAXIMIZED    = 3,
            /// <summary>
            /// Same as SW_SHOWMAXIMIZED
            /// </summary>
            SW_MAXIMIZE         = 3,
            /// <summary>
            /// Show the window but don't activate it.
            /// </summary>
            SW_SHOWNOACTIVATE   = 4,
            /// <summary>
            /// Show the window.
            /// </summary>
            SW_SHOW             = 5,
            /// <summary>
            /// Show the window minimized.
            /// </summary>
            SW_MINIMIZE         = 6,
            /// <summary>
            /// Show minimized and not activated.
            /// </summary>
            SW_SHOWMINNOACTIVE  = 7,
            /// <summary>
            /// Same as SW_SHOWNOACTIVATE
            /// </summary>
            SW_SHOWNA           = 8,
            /// <summary>
            /// Show window restored.
            /// </summary>
            SW_RESTORE          = 9,
            /// <summary>
            /// Show window as a default.
            /// </summary>
            SW_SHOWDEFAULT      = 10,
        }
		
        /// <summary>
        /// Return codes from the static functions.
        /// </summary>
		/// <remarks>These map 1:1 to the Win32 implementation of this enum.</remarks>
		public enum ShellExecuteReturnCodes
        {
            /// <summary>
            /// Out of memory exception
            /// </summary>
            ERROR_OUT_OF_MEMORY			= 0,
            /// <summary>
            /// File not found exception
            /// </summary>
            ERROR_FILE_NOT_FOUND		= 2,
            /// <summary>
            /// Path not found exception
            /// </summary>
            ERROR_PATH_NOT_FOUND		= 3,
            /// <summary>
            /// Bad formatted file
            /// </summary>
            ERROR_BAD_FORMAT			= 11,
            /// <summary>
            /// Access is denied
            /// </summary>
            SE_ERR_ACCESSDENIED			= 5,
            /// <summary>
            /// File association is incomplete
            /// </summary>
            SE_ERR_ASSOCINCOMPLETE		= 27,
            /// <summary>
            /// DDE is busy
            /// </summary>
            SE_ERR_DDEBUSY				= 30,
            /// <summary>
            /// DDE Failed
            /// </summary>
            SE_ERR_DDEFAIL				= 29,
            /// <summary>
            /// DDE Timed out
            /// </summary>
            SE_ERR_DDETIMEOUT			= 28,
            /// <summary>
            /// DLL not found
            /// </summary>
            SE_ERR_DLLNOTFOUND			= 32,
            /// <summary>
            /// Unknown
            /// </summary>
            SE_ERR_FNF					= 2,
            /// <summary>
            /// No file association
            /// </summary>
            SE_ERR_NOASSOC				= 31,
            /// <summary>
            /// Unknown
            /// </summary>
            SE_ERR_OOM					= 8,
            /// <summary>
            /// Unknown
            /// </summary>
            SE_ERR_PNF					= 3,
            /// <summary>
            /// Share violation
            /// </summary>
            SE_ERR_SHARE				= 26,
        }

        /// <summary>
        /// Open verb.
        /// </summary>
        /// <remarks>This string represents the shell open command.</remarks>
        public const string OpenFile		= "open";
        /// <summary>
        /// Edit verb.
        /// </summary>
        /// <remarks>This string represents the shell edit command.</remarks>
        public const string EditFile		= "edit";
        /// <summary>
        /// Explore verb.
        /// </summary>
        /// <remarks>This string represents the shell explore command.</remarks>
        public const string ExploreFolder	= "explore";
        /// <summary>
        /// Find verb.
        /// </summary>
        /// <remarks>This string represents the shell find command.</remarks>
        public const string FindInFolder	= "find";
        /// <summary>
        /// Print verb.
        /// </summary>
        /// <remarks>This string represents the shell print command.</remarks>
        public const string PrintFile		= "print";

        [DllImport("shell32.dll")]
        private static extern IntPtr ShellExecuteW(IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPTStr)]String lpOperation,
            [MarshalAs(UnmanagedType.LPTStr)]String lpFile,
            [MarshalAs(UnmanagedType.LPTStr)]String lpParameters,
            [MarshalAs(UnmanagedType.LPTStr)]String lpDirectory,
            Int32 nShowCmd);

        /// <summary>
        /// General shell execute function.
        /// </summary>
        /// <param name="parent">Optional. Parent window.</param>
        /// <param name="verb">Optional. verb to execute (one of open, edit, explore, find, or print).
        /// null = open</param>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
        /// <remarks>Use this method for complete control over how the shell command is executed.</remarks>
        public static ShellExecuteReturnCodes Execute(IWin32Window parent, string verb, string file, string paramaters, string directory, ShowWindowCommands show)
        {
            IntPtr handle = IntPtr.Zero;
            if(parent != null)
            {
                handle = parent.Handle;
            }
            if(verb == null || verb == "")
            {
                verb = OpenFile;
            }
            if(file == null || file == "")
            {
                throw new ArgumentException("You must specify a file.", "file");
            }

            IntPtr result = ShellExecuteW(handle, verb, file, paramaters, directory, (int)show);

            return (ShellExecuteReturnCodes)result.ToInt32();
        }

        /// <summary>
        /// Execute the 'open' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
        /// <remarks>This function is overloaded.</remarks>
        public static ShellExecuteReturnCodes Open(string file, string paramaters, string directory, ShowWindowCommands show)
        {
            return Execute(null, OpenFile, file, paramaters, directory, show);
        }

        /// <summary>
        /// Execute the 'open' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Open(string file, string paramaters)
        {
            return Open(file, paramaters, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'open' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Open(string file)
        {
            return Open(file, null, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'edit' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Edit(string file, string paramaters, string directory, ShowWindowCommands show)
        {
            return Execute(null, EditFile, file, paramaters, directory, show);
        }

        /// <summary>
        /// Execute the 'edit' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Edit(string file, string paramaters)
        {
            return Edit(file, paramaters, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'edit' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Edit(string file)
        {
            return Edit(file, null, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'explore' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Explore(string file, string paramaters, string directory, ShowWindowCommands show)
        {
            return Execute(null, ExploreFolder, file, paramaters, directory, show);
        }

        /// <summary>
        /// Execute the 'explore' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Explore(string file, string paramaters)
        {
            return Explore(file, paramaters, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'explore' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Explore(string file)
        {
            return Explore(file, null, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'print' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Print(string file, string paramaters, string directory, ShowWindowCommands show)
        {
            return Execute(null, PrintFile, file, paramaters, directory, show);
        }

        /// <summary>
        /// Execute the 'print' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Print(string file, string paramaters)
        {
            return Print(file, paramaters, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'print' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Print(string file)
        {
            return Print(file, null, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'find' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <param name="directory">Optional. Default directory to execute the file parameter in. Can be null.</param>
        /// <param name="show">How to launch the program/document.  One of Shell.ShowWindowCommands.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Find(string file, string paramaters, string directory, ShowWindowCommands show)
        {
            return Execute(null, FindInFolder, file, paramaters, directory, show);
        }

        /// <summary>
        /// Execute the 'find' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <param name="paramaters">Optional.  Parameters if the file parameter is a program.
        /// null if file parameter is a document or there are no parameters.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Find(string file, string paramaters)
        {
            return Find(file, paramaters, null, ShowWindowCommands.SW_NORMAL);
        }

        /// <summary>
        /// Execute the 'find' verb on the paramters.
        /// </summary>
        /// <param name="file">Required. Program or document to launch.</param>
        /// <returns>The result of the ShellExecute API as a Shell.ShellExecuteReturnCodes.</returns>
		/// <remarks>This function is overloaded.</remarks>
		public static ShellExecuteReturnCodes Find(string file)
        {
            return Find(file, null, null, ShowWindowCommands.SW_NORMAL);
        }
    }
}
