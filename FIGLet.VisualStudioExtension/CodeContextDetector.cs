using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using System.Threading.Tasks;
using System;
using System.IO;

namespace FIGLet.VisualStudioExtension
{
    public class CodeContextDetector
    {
        private readonly AsyncPackage _package;

        public CodeContextDetector(AsyncPackage package)
        {
            _package = package;
        }

        public async Task<InsertionContext> GetInsertionContextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await _package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte?.ActiveDocument == null) return null;

            try
            {
                // Get current selection
                var selection = dte.ActiveDocument.Selection as TextSelection;
                if (selection == null) return null;

                // Try to get containing class/method using CodeModel
                var codeElement = GetContainingCodeElement(selection);
                if (codeElement != null)
                {
                    switch (codeElement.Kind)
                    {
                        case vsCMElement.vsCMElementClass:
                            return new InsertionContext
                            {
                                Type = ContextType.Class,
                                Name = codeElement.Name,
                                StartPoint = codeElement.StartPoint,
                                EndPoint = codeElement.EndPoint
                            };

                        case vsCMElement.vsCMElementFunction:
                            return new InsertionContext
                            {
                                Type = ContextType.Method,
                                Name = codeElement.Name,
                                StartPoint = codeElement.StartPoint,
                                EndPoint = codeElement.EndPoint
                            };
                    }
                }

                // If no specific context found, return file context
                var textDoc = dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (textDoc != null)
                {
                    return new InsertionContext
                    {
                        Type = ContextType.File,
                        Name = Path.GetFileName(dte.ActiveDocument.FullName),
                        StartPoint = textDoc.StartPoint,
                        EndPoint = textDoc.EndPoint
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log exception - CodeModel might not be available for all languages
                System.Diagnostics.Debug.WriteLine($"Error detecting code context: {ex}");
                return null;
            }
        }

        private CodeElement GetContainingCodeElement(TextSelection selection)
        {
            try
            {
                CodeElement element = null;

                // Try to get the immediate code element at cursor
                if (selection != null)
                {
                    element = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementFunction);
                    if (element != null) return element;

                    // If no method found, try to get containing class
                    element = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementClass);
                    if (element != null) return element;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class InsertionContext
    {
        public ContextType Type { get; set; }
        public string Name { get; set; }
        public TextPoint StartPoint { get; set; }
        public TextPoint EndPoint { get; set; }

        public bool TryGetPreferredInsertionPoint(InsertLocation preference, out TextPoint insertPoint)
        {
            insertPoint = null;

            try
            {
                switch (preference)
                {
                    case InsertLocation.Above:
                        insertPoint = StartPoint;
                        return true;

                    case InsertLocation.Below:
                        insertPoint = EndPoint;
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public enum ContextType
    {
        File,
        Class,
        Method
    }
}