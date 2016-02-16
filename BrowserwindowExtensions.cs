using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using System;
using System.Runtime.InteropServices;


namespace FluentBytes
{
    public static class BrowserWindowExtensions
    {
        public static T FindElement<T>(this UITestControl container, Func<UITestControl, HtmlControl, HtmlControl> controlConstructorFunct) where T : HtmlControl, new()
        {
            var control = new T { Container = container };
            controlConstructorFunct(container, control);
            return control;
        }
        public static T FindElement<T>(this BrowserWindow container, Func<UITestControl, HtmlControl, HtmlControl> controlConstructorFunct) where T : HtmlControl, new()
        {
            var control = new T { Container = container };
            controlConstructorFunct(container, control);
            return control;
        }
        public static UITestControlCollection FindElements<T>(this UITestControl container, Func<UITestControl, T> controlsConstructorFunct) where T : HtmlControl
        {
            return controlsConstructorFunct(container).FindMatchingControls();
        }

        public static void Click(this UITestControl control)
        {
            Mouse.Click(control);
        }

        public static void SendKeys(this UITestControl control, string text)
        {
            Keyboard.SendKeys(control, text);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public static void resizeWindow(this BrowserWindow control, int width, int height)
        {
            //Call the native method to resize the window
            SetWindowPos(control.WindowHandle, (IntPtr)(-1), 0, 0, width, height, 0x0002 | 0x0040);
        }

    }

    public class By
    {

        public static Func<UITestControl, HtmlControl, HtmlControl> Id(string id)
        {
            return (container, control) =>
            {
                control.SearchProperties.Add(HtmlControl.PropertyNames.Id, id);
                return control;
            };
        }

        public static Func<UITestControl, HtmlControl, HtmlControl> InnerText(string innerText)
        {
            return InnerText(innerText, PropertyExpressionOperator.EqualTo);
        }

        public static Func<UITestControl, HtmlControl, HtmlControl> InnerText(string innerText, PropertyExpressionOperator expressionOperator)
        {
            return (container, control) =>
            {
                control.SearchProperties.Add(HtmlControl.PropertyNames.InnerText, innerText, expressionOperator);
                return control;
            };
        }


        public static Func<UITestControl, HtmlControl, HtmlControl> ClassName(string classNameToFind)
        {
            return (container,control) =>
            {
               control.SearchProperties.Add(UITestControl.PropertyNames.ClassName, classNameToFind);
                return control;
            };
        }
        public static Func<UITestControl, HtmlControl, HtmlControl> CssSelector(string cssSelectorToFind)
        {
            // for CSS selectors, I prefer to use a Javascript function.
            // this then utilizes the fastest way to retrieve it using javascript and
            // gives all flexibility in the selector. using only the Class would not yield the same results as
            // we see with e.g. selenium.

            const string javascript = "return document.querySelector('{0}');";
            var scriptToExecute = string.Format(javascript, cssSelectorToFind);
            return (container, control) =>
            {
                var browserWindow = container as BrowserWindow;
                if (browserWindow == null)
                    throw new ArgumentException("You can only use the CSSSelector function on a control of type BrowserWindow");

                var searchControl = browserWindow.ExecuteScript(scriptToExecute) as HtmlControl;

                var foundControltype = searchControl?.GetType();
                var returnType = control.GetType();
                if (foundControltype?.FullName == returnType.FullName)
                {
                    control = searchControl;
                }
                else
                {
                    throw new InvalidCastException(
                        $"Unable to assign control found to type {returnType.FullName}, control is of type {foundControltype?.FullName}");
                }
                return control;
            };
        }

        public static Func<UITestControl, HtmlControl, HtmlControl> Name(string nameToFind)
        {
            return (container, control) =>
            {
                control.SearchProperties.Add(UITestControl.PropertyNames.Name, nameToFind);
                return control;
            };
        }
        public static Func<UITestControl, HtmlControl, HtmlControl> LinkText(string linkTextToFind)
        {
            return InnerText(linkTextToFind);
        }
        public static Func<UITestControl, HtmlControl, HtmlControl> PartialLinkText(string partialLinkTextToFind)
        {
            return InnerText(partialLinkTextToFind, PropertyExpressionOperator.Contains);
        }

    }
}
