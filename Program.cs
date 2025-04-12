using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Consoleprueba
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configuración del reporte
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "TestReport.html");
            var htmlReporter = new ExtentSparkReporter(reportPath);
            var extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);

            // Crear un caso de prueba en el reporte
            var testSuccess = extent.CreateTest("Prueba de Login - Caso de Éxito");
            var testFailure = extent.CreateTest("Prueba de Login - Caso de Fracaso");

            IWebDriver driver = null;

            try
            {
                // Configuración del Edge WebDriver
                var options = new EdgeOptions();
                driver = new EdgeDriver(options);

                #region Caso de Éxito
                // Prueba de inicio de sesión exitoso
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/login");
                driver.Manage().Window.Maximize();

                // Capturar pantalla inicial
                string screenshotPath = CaptureScreenshot(driver, "LoginPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");

                // Automatizar el formulario de inicio de sesión con credenciales correctas
                driver.FindElement(By.Id("username")).SendKeys("tomsmith");
                driver.FindElement(By.Id("password")).SendKeys("SuperSecretPassword!");
                driver.FindElement(By.ClassName("radius")).Click();

                // Capturar pantalla después de iniciar sesión
                screenshotPath = CaptureScreenshot(driver, "LoggedInPage");
                testSuccess.AddScreenCaptureFromPath(screenshotPath, "Página después del inicio de sesión");

                // Verificar mensaje de éxito
                var successMessage = driver.FindElement(By.CssSelector(".flash.success")).Text;
                if (successMessage.Contains("You logged into a secure area!"))
                {
                    testSuccess.Pass("Inicio de sesión exitoso: " + successMessage);
                }
                else
                {
                    testSuccess.Fail("Inicio de sesión fallido.");
                }
                #endregion

                #region Caso de Fracaso
                // Prueba de inicio de sesión fallido
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegando a la página de inicio de sesión");
                driver.Navigate().GoToUrl("https://the-internet.herokuapp.com/login");
                driver.Manage().Window.Maximize();

                // Capturar pantalla inicial
                screenshotPath = CaptureScreenshot(driver, "LoginPage_Failure");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página de inicio de sesión");

                // Automatizar el formulario de inicio de sesión con credenciales incorrectas
                driver.FindElement(By.Id("username")).SendKeys("usuarioIncorrecto");
                driver.FindElement(By.Id("password")).SendKeys("claveIncorrecta");
                driver.FindElement(By.ClassName("radius")).Click();

                // Capturar pantalla después del intento de inicio de sesión
                screenshotPath = CaptureScreenshot(driver, "FailedLoginAttempt");
                testFailure.AddScreenCaptureFromPath(screenshotPath, "Página después del intento de inicio de sesión");

                // Verificar mensaje de error
                try
                {
                    var errorMessage = driver.FindElement(By.CssSelector(".flash.error")).Text;
                    if (errorMessage.Contains("Your username is invalid!"))
                    {
                        testFailure.Pass("Inicio de sesión fallido como se esperaba: " + errorMessage);
                    }
                    else
                    {
                        testFailure.Fail("El mensaje de error no es el esperado.");
                    }
                }
                catch (NoSuchElementException)
                {
                    testFailure.Fail("No se encontró el mensaje de error esperado.");
                }
                #endregion
            }
            catch (Exception ex)
            {
                // Si ocurre un error general
                testFailure.Fail("Error durante la prueba: " + ex.Message);
            }
            finally
            {
                // Cerrar el navegador
                driver?.Quit();
                testSuccess.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");
                testFailure.Log(AventStack.ExtentReports.Status.Info, "Navegador cerrado");

                // Generar el reporte
                extent.Flush();
            }
        }

        // Método para capturar capturas de pantalla
        static string CaptureScreenshot(IWebDriver driver, string screenshotName)
        {
            try
            {
                string screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                string screenshotPath = Path.Combine(screenshotsDir, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                return screenshotPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al capturar la pantalla: " + ex.Message);
                return null;
            }
        }
    }
}