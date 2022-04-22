using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace esco.report.server
{
    public class BrowserHelper
    {
        /// <summary>
        /// Llame al navegador del sistema para abrir la página web
        /// http://m.jb51.net/article/44622.htm
        /// http://www.2cto.com/kf/201412/365633.html
        /// </summary>
        /// <param name = "url"> Abra el enlace a la página </ param>
        public static void OpenBrowserUrl(string url)
        {
            if (url is null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            // ruta de registro de 64 bits
            var openKey = @"SOFTWARE\Wow6432Node\Google\Chrome";
            try
            {
                if (IntPtr.Size == 4)
                {
                    // ruta de registro de 32 bits
                    openKey = @"SOFTWARE\Google\Chrome";
                }
                // Google Chrome se abre con Google, si no se encuentra, se usa el navegador predeterminado del sistema
                // Google se desinstaló, el registro no se ha borrado, el programa devolverá un mensaje "El sistema no puede encontrar el archivo especificado".
                starChrome(Registry.LocalMachine.OpenSubKey(openKey), url);
            }
            catch
            {
                try
                {
                    starChrome(Registry.LocalMachine.OpenSubKey(openKey), url, " (x86)");
                }
                catch
                {
                    // Llame al navegador predeterminado del usuario si ocurre un error, o llame a IE si falla
                    OpenDefaultBrowserUrl(url);
                }
            }
        }
        private static void starChrome(RegistryKey appPath, string url, string x86 = "")
        {
            string path = "C:\\Program Files" + x86 + "\\Google\\Chrome\\Application\\chrome.exe";
            if (appPath != null)
            {
                var result = Process.Start(path, url);
                if (result == null)
                {
                    OpenIe(url);
                }
            }
            else
            {
                var result = Process.Start(path, url);
                if (result == null)
                {
                    OpenDefaultBrowserUrl(url);
                }
            }
        }

        /// <summary>
        /// Abre el navegador con IE
        /// </summary>
        /// <param name="url"></param>
        public static void OpenIe(string url)
        {
            try
            {
                Process.Start("iexplore.exe", url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // Instalación de la ruta del navegador IE: C: \ Archivos de programa \ Internet Explorer
                // en System.Diagnostics.process.StartWithshellExecuteEx (ProcessStartInfo startInfo) Tenga en cuenta este error
                try
                {
                    if (File.Exists(@"C:\Program Files\Internet Explorer\iexplore.exe"))
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo
                        {
                            FileName = @"C:\Program Files\Internet Explorer\iexplore.exe",
                            Arguments = url,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(processStartInfo);
                    }
                    else
                    {
                        if (File.Exists(@"C:\Program Files (x86)\Internet Explorer\iexplore.exe"))
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo
                            {
                                FileName = @"C:\Program Files (x86)\Internet Explorer\iexplore.exe",
                                Arguments = url,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            Process.Start(processStartInfo);
                        }
                        else
                        {
                            if (MessageBox.Show("El navegador IE no está instalado, ¿descargar e instalar?", null, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                // Abra el enlace de descarga y descárguelo del sitio web oficial de Microsoft
                                OpenDefaultBrowserUrl("http://windows.microsoft.com/zh-cn/internet-explorer/download-ie");
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        /// <summary>
        /// Abra el navegador predeterminado del sistema (el usuario ha configurado el navegador predeterminado)
        /// </summary>
        /// <param name="url"></param>
        public static void OpenDefaultBrowserUrl(string url)
        {
            try
            {
                // método 1
                // Lea la ruta predeterminada del archivo ejecutable del navegador del registro
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
                if (key != null)
                {
                    string s = key.GetValue("").ToString();
                    // s es su navegador predeterminado, pero con los parámetros detrás de él, está truncado, ¡pero debe tenerse en cuenta que los parámetros detrás de los diferentes navegadores son diferentes!
                    //"D:\Program Files (x86)\Google\Chrome\Application\chrome.exe" -- "%1"
                    var lastIndex = s.IndexOf(".exe", StringComparison.Ordinal);
                    if (lastIndex == -1)
                    {
                        lastIndex = s.IndexOf(".EXE", StringComparison.Ordinal);
                    }
                    var path = s.Substring(1, lastIndex + 3);
                    var result = Process.Start(path, url);
                    if (result == null)
                    {
                        // método 2
                        // llama al navegador predeterminado del sistema
                        var result1 = Process.Start("explorer.exe", url);
                        if (result1 == null)
                        {
                            // método 3
                            Process.Start(url);
                        }
                    }
                }
                else
                {
                    // método 2
                    // llama al navegador predeterminado del sistema
                    var result1 = Process.Start("explorer.exe", url);
                    if (result1 == null)
                    {
                        // método 3
                        Process.Start(url);
                    }
                }
            }
            catch
            {
                OpenIe(url);
            }
        }

        /// <summary>
        /// Firefox abre página web
        /// </summary>
        /// <param name="url"></param>
        public static void OpenFireFox(string url)
        {
            try
            {
                // ruta de registro de 64 bits
                var openKey = @"SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox";
                if (IntPtr.Size == 4)
                {
                    // ruta de registro de 32 bits
                    openKey = @"SOFTWARE\Mozilla\Mozilla Firefox";
                }
                RegistryKey appPath = Registry.LocalMachine.OpenSubKey(openKey);
                if (appPath != null)
                {
                    var result = Process.Start("firefox.exe", url);
                    if (result == null)
                    {
                        OpenIe(url);
                    }
                }
                else
                {
                    var result = Process.Start("firefox.exe", url);
                    if (result == null)
                    {
                        OpenDefaultBrowserUrl(url);
                    }
                }
            }
            catch
            {
                OpenDefaultBrowserUrl(url);
            }
        }
    }
}
