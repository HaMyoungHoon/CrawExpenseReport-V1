using BaseLib_Net5;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawExpenseReport.Base.Flow
{
    public static class StartFlow
    {
        public static bool DriverSet(ref EdgeDriver driver)
        {
//            if (driver != null)
//            {
//                try
//                {
//                    driver.Quit();
//                    driver = null;
//                }
//                catch
//                {
//
//                }
//            }

            string edgeDriver = FBaseFunc.Ins.Cfg.WebDriverPath;
            try
            {
                if (driver == null)
                {
                    EdgeDriverService service = EdgeDriverService.CreateChromiumService(edgeDriver);
                    service.HideCommandPromptWindow = true;
                    EdgeOptions options = new()
                    {
                        UseChromium = true,
                        BinaryLocation = FBaseFunc.Ins.Cfg.EdgePath,
                    };
                    driver = new EdgeDriver(service, options);
                    driver.Manage().Window.Maximize();
                }
                else
                {
                    if (driver.WindowHandles.Count > 0)
                    {
                        driver.ExecuteScript("window.open();");
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    else
                    {
                        EdgeDriverService service = EdgeDriverService.CreateChromiumService(edgeDriver);
                        service.HideCommandPromptWindow = true;
                        EdgeOptions options = new()
                        {
                            UseChromium = true,
                            BinaryLocation = FBaseFunc.Ins.Cfg.EdgePath,
                        };
                        driver = new EdgeDriver(service, options);
                    }
                }
            }
            catch (Exception ex)
            {
                FBaseFunc.Ins.SetResultMethod("이전 브라우저를 종료했거나, 탭을 종료한 것 같습니다. 다시 시도해주세요.");
                FBaseFunc.Ins.SetResultMethod(ex.Message);
                FBaseFunc.Ins.SetLog(ex.Message);
                driver = null;
                return false;
            }

            return true;
        }
        public static bool Login(EdgeDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(driver.Url = FBaseFunc.Ins.DefaultURL());
                if (!driver.Url.Contains("app/home"))
                {
                    IWebElement id = FindElement(driver, FBaseFunc.Ins.Cfg.LoginSector.ID, FBaseFunc.ElementType.ID);
                    IWebElement pw = FindElement(driver, FBaseFunc.Ins.Cfg.LoginSector.PW, FBaseFunc.ElementType.ID);
                    IWebElement login = FindElement(driver, FBaseFunc.Ins.Cfg.BtnLogin, FBaseFunc.ElementType.ID);
                    id.SendKeys(FBaseFunc.Ins.Cfg.LoginInfo.ID);
                    pw.SendKeys(FBaseFunc.Ins.Cfg.LoginInfo.PW);
                    login.SendKeys(Keys.Return);
                    
                    Thread.Sleep(1000);
                    if (driver.Url.ToUpper().Contains("LOGIN"))
                    {
                        string loginErr = FindElement(driver, FBaseFunc.Ins.Cfg.LoginErr, FBaseFunc.ElementType.CLASS).GetAttribute("style");
                        if (loginErr.Contains("display: none;"))
                        {
                            string err = FindElement(driver, FBaseFunc.Ins.Cfg.LoginErr, FBaseFunc.ElementType.CLASS).FindElement(By.ClassName("txt")).Text;
                            FBaseFunc.Ins.SetResultMethod(err);
                            FBaseFunc.Ins.SetLog(err);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FBaseFunc.Ins.SetResultMethod(ex.Message);
                FBaseFunc.Ins.SetLog(ex.Message);
                return false;
            }

            return true;
        }

        private static IWebElement FindElement(EdgeDriver driver, string data, FBaseFunc.ElementType type, int timeout = 0)
        {
            if (timeout == 0)
            {
                timeout = FBaseFunc.Ins.Cfg.Timeout;
            }

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(timeout));
            return type switch
            {
                FBaseFunc.ElementType.XPATH => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(data))),
                FBaseFunc.ElementType.ID => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(data))),
                FBaseFunc.ElementType.CLASS => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName(data))),
                _ => null,
            };
        }
    }
}
