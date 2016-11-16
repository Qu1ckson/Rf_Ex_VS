using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Qu1ckson.rf_ex_vs
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidrf_ex_vsPkgString)]
    public sealed class rf_ex_vsPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public rf_ex_vsPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        private static int SIZE_VAR_NAME = 10;
        private static int SIZE_FUNC_NAME = 15;


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidrf_ex_vsCmdSet, (int)PkgCmdIDList.cmdidMyCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidrf_ex_vsCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }
        #endregion

        private void ErrorMsg( String msg )
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Error",
                       msg,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_WARNING,
                       0,        // false
                       out result));
        }

        void CreateSetterFunction(VCCodeElement elem, FileCodeModel model)
        {
            // TODO CHECK!
            VCCodeClass elem_par = elem.Parent;
            
            string full_name = elem.FullName;
            string[] seps = { "::" };
            //TODO проверить на кол-во элементов
            string[] class_var = full_name.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);

            String name_set = /*class_var[0] + "::" +*/ char.ToUpper(class_var[1][0]) + class_var[1].Substring(1);

            VCCodeFunction func_set = (VCCodeFunction)elem_par.AddFunction(name_set, vsCMFunction.vsCMFunctionTopLevel, "int", -1, vsCMAccess.vsCMAccessPublic);
            func_set.DeclarationText += ";";

            VCCodeVariable my_var = elem as VCCodeVariable;

            func_set.AddParameter("value", my_var.TypeString, -1);
        }

        void CreateGetterFunction( CodeElement elem, FileCodeModel model )
        {
            
        }

        bool IsCanRename(string name)
        {
            //return false;
            if (name == "main")
                return false;
            if (name == "printf")
                return false;
            return true;
        }
        
        private void Obfuscation()
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            Document active_doc = dte.ActiveDocument;
            VCFileCodeModel code_model_active_doc = null;
            if (active_doc.ProjectItem != null && active_doc.ProjectItem.FileCodeModel != null)
                code_model_active_doc = (VCFileCodeModel)active_doc.ProjectItem.FileCodeModel;

            if (code_model_active_doc == null)
                return;

            TextDocument text_doc = (TextDocument)active_doc.Object("TextDocument");
            foreach (CodeElement2 elem in code_model_active_doc.CodeElements)
            {
                //elem. = "";
                if (elem.Kind == vsCMElement.vsCMElementVariable)
                {
                    string old_name = elem.Name;
                    string new_name = RandName(SIZE_VAR_NAME);
                    CodeTypeRef refd = ((EnvDTE.CodeVariable)elem).Type;
                    if (IsCanRename(old_name))
                        elem.RenameSymbol(new_name);
                    //text_doc.ReplacePattern(old_name, new_name, (int)vsFindOptions.vsFindOptionsMatchCase | (int)vsFindOptions.vsFindOptionsMatchWholeWord);
                    
                }
                else if (elem.Kind == vsCMElement.vsCMElementFunction)
                {
                    string old_name = elem.Name;
                    string new_name = RandName(SIZE_FUNC_NAME);

                    VCCodeFunction func = (VCCodeFunction)elem;
                    //func.Comment = "";
                    if (IsCanRename(old_name))
                    {
                        elem.RenameSymbol(new_name);
                        text_doc.ReplacePattern(old_name, new_name, (int)vsFindOptions.vsFindOptionsMatchCase | (int)vsFindOptions.vsFindOptionsMatchWholeWord);
                    }
                    ObfuscationFunc( ref func );
                }

            }

            text_doc.ReplacePattern(@"//(.*?)\r?\n", " ", (int)vsFindOptions.vsFindOptionsRegularExpression);
            text_doc.ReplacePattern("\n", " ");
            text_doc.ReplacePattern("\t", " ");
        }

        private readonly Random _rng = new Random();


        private string RandName( int size )
        {
            const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }
        static ProjectItem projectItem = null;
        private void ObfuscationFunc( ref VCCodeFunction func )
        {
            ProjectItem pi = null;
            try
            {
                if (func == null)
                    return;
                EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
                string body_text = func.BodyText;

                string file_name = RandName(10);
                file_name += ".cpp";

                string file_path = @"D:\\";
                file_path += file_name;

                Project proj;
                Solution2 soln;
                soln = (Solution2)dte.Solution;

                System.IO.StreamWriter textFile = new System.IO.StreamWriter(file_path);
                textFile.Write(body_text);
                textFile.Close();

                if (projectItem == null)
                {

                    
                    proj = soln.Projects.Item(1);
                    proj.ProjectItems.AddFromFileCopy(file_path);
                    //proj.ProjectItems.AddFromFile(file_path);
                    pi = soln.FindProjectItem(file_name);
                    for (int i = 0; i < 1000000; i++) ;
                        //pi = proj.ProjectItems.Item(file_name);
                        if (pi == null)
                        {
                            foreach (ProjectItem proj_item in proj.ProjectItems)
                            {
                                pi = proj_item.ProjectItems.Item(file_name);
                                if (pi != null)
                                {
                                    //projectItem = proj_item;
                                    break;
                                }
                            }
                        }
                }
                else
                    pi = projectItem.ProjectItems.Item( file_name );


                if (pi == null)
                    return;

                foreach (CodeElement2 elem in func.Children)
                {
                    if (elem.Kind == vsCMElement.vsCMElementParameter)
                    {
                        string old_name = elem.Name;
                        string new_name = RandName(SIZE_VAR_NAME);
                        string input = "\\b" + old_name + "\\b";
                        elem.RenameSymbol(new_name);
                        body_text = Regex.Replace(body_text, input, new_name);
                    }
                }

                FileCodeModel code_model = (FileCodeModel)pi.FileCodeModel;
                if (code_model == null)
                {
                    pi = soln.FindProjectItem(file_name);
                    code_model = (FileCodeModel)pi.FileCodeModel;
                    if( code_model == null )
                        return;
                }
                    //return;

                Dictionary<string, string> renames = new Dictionary<string, string>();
                foreach (CodeElement2 v in code_model.CodeElements)
                {
                    if (v.Kind == vsCMElement.vsCMElementVariable)
                    {
                        CodeTypeRef type = ((EnvDTE.CodeVariable)v).Type;
                        if (type == null)
                            continue;
                        if (IsCanRename(v.Name))
                        {
                            string old_name = v.Name;
                            string new_name = RandName(SIZE_VAR_NAME);
                            renames.Add(old_name, new_name);
                            v.RenameSymbol(new_name);
                            string input = "\\b" + old_name + "\\b";
                            body_text = Regex.Replace(body_text, input, new_name);
                            //body_text = body_text.Replace(old_name, new_name);
                        }
                    }
                }
                //pi.Remove();
                func.BodyText = body_text;
            }
            finally
            {
                if(pi != null)
                    pi.Remove();
            }
            //string text = System.IO.File.ReadAllText(@"D:\\MyFile.cpp");

            //soln.SaveAs(soln.FullName);
           // proj.Save();
            //pi.Delete();
            //if (!proj.Saved)
            //    proj.Save();
            
            //proj.Save();
            //System.IO.File.Delete(@"D:\\MyFile.cpp");

        }
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            Obfuscation();
            //EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            //Document active_doc = dte.ActiveDocument;
            //FileCodeModel code_model_active_doc = null;
            //if (active_doc.ProjectItem != null && active_doc.ProjectItem.FileCodeModel != null)
            //    code_model_active_doc = (FileCodeModel)active_doc.ProjectItem.FileCodeModel;

            //if (code_model_active_doc == null)
            //    return;

            //Project proj = dte.Solution.Item(1);

            //foreach (ProjectItem v in dte.Solution.Item(1).ProjectItems)
            //{
            //    string str = v.Name;
            //}


            //ProjectItem proj_file = dte.Solution.Item(1).ProjectItems.Item("stdafx.cpp");
            ////ProjectItem proj_file = FindProjectItemInProject (dte.Solution.Item(1), "stdafx.cpp");

            //FileCodeModel proj_file_code_model = proj_file.FileCodeModel;
            ////proj_file.

            //proj_file_code_model.AddFunction("hello_world::Set", vsCMFunction.vsCMFunctionFunction, "void", -1);

            //if (!(active_doc.Selection is TextSelection))
            //    return;

            //TextSelection text_selection = active_doc.Selection as TextSelection;
            //if (text_selection.Text.Length == 0)
            //{
            //    ErrorMsg("Нужно выбрать переменную!");
            //    return;
            //}

            //VCCodeElement elem = (VCCodeElement)code_model_active_doc.CodeElementFromPoint(text_selection.ActivePoint, vsCMElement.vsCMElementVariable);
            //if (elem == null)
            //    return;
            //if (elem.Kind != vsCMElement.vsCMElementVariable)
            //{
            //    ErrorMsg("Неверный тип выделенного объекта!");
            //    return;
            //}

            //CreateSetterFunction(elem, code_model_active_doc);

        //    // Show a Message Box to prove we were here
        //    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
        //    Guid clsid = Guid.Empty;
        //    int result;
        //    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
        //               0,
        //               ref clsid,
        //               "rf_ex_vs",
        //               string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
        //               string.Empty,
        //               0,
        //               OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
        //               OLEMSGICON.OLEMSGICON_INFO,
        //               0,        // false
        //               out result));
        }

    }
}
