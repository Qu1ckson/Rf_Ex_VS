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

namespace Qu1ckson.rf_ex_vs.src
{
    class Obfuscator
    {
        //public void Start()
        //{
        //    EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
        //    Document active_doc = dte.ActiveDocument;
        //    FileCodeModel code_model_active_doc = null;
        //    if (active_doc.ProjectItem != null && active_doc.ProjectItem.FileCodeModel != null)
        //        code_model_active_doc = (FileCodeModel)active_doc.ProjectItem.FileCodeModel;

        //    if (code_model_active_doc == null)
        //        return;

        //    foreach (CodeElement v in code_model_active_doc.CodeElements)
        //    {
        //        CodeTypeRef refd = ((EnvDTE.CodeVariable)v).Type;
        //        v.Name = RandName(5);
        //    }
        //}
    }
}
