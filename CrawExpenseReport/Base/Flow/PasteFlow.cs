using BaseLib_Net5;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawExpenseReport.Base.Flow
{
    public static class PasteFlow
    {
        public static bool OpenApproval(ref EdgeDriver driver)
        {
            try
            {
                string url = FBaseFunc.Ins.DefaultURL();
                if (url[url.Length - 1] != '/')
                {
                    url = url + "/";
                }

                driver.Navigate().GoToUrl(url + FBaseFunc.Ins.Cfg.ApprovalUrl);

                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                FindElement(driver, FBaseFunc.Ins.Cfg.BtnNew, FBaseFunc.ElementType.XPATH).Click();
                IWebElement input = FindElement(driver, FBaseFunc.Ins.Cfg.TbSearchNew, FBaseFunc.ElementType.XPATH);
                string expenseReportName = FBaseFunc.Ins.Cfg.ExpenseReportName;
                foreach (var name in expenseReportName)
                {
                    input.SendKeys(name.ToString());
                }
                input.SendKeys(Keys.Return);
                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                FindElement(driver, FBaseFunc.Ins.Cfg.BodyNew, FBaseFunc.ElementType.XPATH).Click();
                FindElement(driver, FBaseFunc.Ins.Cfg.BtnConfirm, FBaseFunc.ElementType.XPATH).Click();
                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
            }
            catch (Exception ex)
            {
                FBaseFunc.Ins.SetResultMethod(ex.Message);
                FBaseFunc.Ins.SetLog(ex.Message);

                return false;
            }

            return true;
        }

        public static bool PasteExpenseReort(EdgeDriver driver)
        {
            switch (FBaseFunc.Ins.Cfg.IsSpeedUp)
            {
                case true:
                    {
                        switch (FBaseFunc.Ins.Cfg.IsDirectInput)
                        {
                            case true: return PasteExpenseReportDirectSpeedUp(driver);
                            default: return PasteExpenseReportDefulatSpeedUp(driver);
                        }
                    }
                case false:
                    {
                        switch (FBaseFunc.Ins.Cfg.IsDirectInput)
                        {
                            case true: return PasteExpenseReportDirect(driver);
                            case false: return PasteExpenseReportDefulat(driver);
                        }
                    }
            }
        }

        public static bool PasteExpenseReportDefulat(EdgeDriver driver)
        {
            if (FBaseFunc.Ins.SelectedList == -1)
            {
                FBaseFunc.Ins.SetResultMethod("선택된 리스트가 없습니다.");

                return false;
            }

            FBaseFunc.CopySectorTable copySector = FBaseFunc.Ins.Cfg.CopySector;
            FBaseFunc.CopyDataTable copyData = FBaseFunc.Ins.CopyedTable[FBaseFunc.Ins.SelectedList];
            FBaseFunc.PasteSectorTable pasteSector = FBaseFunc.Ins.Cfg.PasteSector;

            bool isSuccessLogHide = FBaseFunc.Ins.Cfg.IsSuccessLogHide;

            try
            {
                try
                {
                    IWebElement title = FindElement(driver, copySector.Title, FBaseFunc.ElementType.ID);
                    title.SendKeys(copyData.Title);
                }
                catch (Exception ex)
                {
                    FBaseFunc.Ins.SetResultMethod("제목을 찾지 못했습니다.");
                    FBaseFunc.Ins.SetResultMethod(ex.Message);
                    FBaseFunc.Ins.SetLog(ex.Message);
                }
                // 회사
                if (copyData.CompanyValue.CompanyCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement company = FindElement(driver, copySector.CompanySector.CompanyCode, FBaseFunc.ElementType.CLASS).FindElement(By.TagName("input"));
                    if (company.GetAttribute("data-code") != copyData.CompanyValue.CompanyCode)
                    {
                        company.Click();
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);

                        ReadOnlyCollection<IWebElement> bodys = FindElement(driver, pasteSector.Company, FBaseFunc.ElementType.XPATH).FindElements(By.TagName("tr"));
                        bool find = false;
                        foreach (var body in bodys)
                        {
                            if (body.GetAttribute("data-code") == copyData.CompanyValue.CompanyCode)
                            {
                                body.Click();
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t회사 : {0}", copyData.CompanyValue.CompanyCode), isSuccessLogHide);
                                find = true;
                                break;
                            }
                        }

                        if (find == false)
                        {
                            FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID).Click();
                            FBaseFunc.Ins.SetResultMethod("사명을 못 찾았습니다.");
                            FBaseFunc.Ins.SetLog("[PasteFlow]\t사명을 못 찾았습니다.");
                            return false;
                        }
                    }
                }

                // 사업장
                if (copyData.WorkplaceValue.WorkplaceCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement workplace = FindElement(driver, copySector.WorkplaceSector.WorkplaceCode, FBaseFunc.ElementType.CLASS).FindElement(By.TagName("input"));
                    if (workplace.GetAttribute("data-code") != copyData.WorkplaceValue.WorkplaceCode)
                    {
                        workplace.Click();
                        Thread.Sleep(100);

                        ReadOnlyCollection<IWebElement> bodys = FindElement(driver, pasteSector.Workplace, FBaseFunc.ElementType.XPATH).FindElements(By.TagName("td"));
                        bool find = false;
                        foreach (var body in bodys)
                        {
                            if (body.Text == copyData.WorkplaceValue.WorkplaceCode)
                            {
                                body.Click();
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사업장 : {0}", copyData.WorkplaceValue.WorkplaceCode), isSuccessLogHide);
                                find = true;
                                break;
                            }
                        }

                        if (find == false)
                        {
                            FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID).Click();
                            FBaseFunc.Ins.SetResultMethod("사업장을 못 찾았습니다.");
                            FBaseFunc.Ins.SetLog("[PasteFlow]\t사업장을 못 찾았습니다.");
                            return false;
                        }
                    }
                }

                int slipCount = copyData.SlipValues.Count;
                int retryCount = FBaseFunc.Ins.Cfg.RetryCount;
                // 행 추가
                if (FBaseFunc.Ins.Cfg.IsNewFirst)
                {
                    for (int i = 0; i < slipCount - 1; i++)
                    {
                        try
                        {
                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                            IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                            btnAddRow.Click();
                        }
                        catch (Exception ex)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetLog(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetResultMethod(ex.Message);
                            FBaseFunc.Ins.SetLog(ex.Message);
                        }
                    }
                }

                // 전표 테이블
                ReadOnlyCollection<IWebElement> slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                for (int i = 0; i < slipCount; i++)
                {
                    if (FBaseFunc.Ins.IsTerminateOn)
                    {
                        FBaseFunc.Ins.SetLog("TerminateOn");
                        return false;
                    }

                    bool retRetry = false;
                    // 행 추가
                    if (i >= slipBodys.Count / 2)
                    {
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                btnAddRow.Click();
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t열추가 {0}회 시도 실패", retryCount));
                            return false;
                        }
                        slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                    }

                    FBaseFunc.SlipTable slipValue = copyData.SlipValues[i];

                    // 구분 찾기
                    if (slipValue.Gubun != "차변" && copySector.SlipSector.IsEmptyGubun() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                ReadOnlyCollection<IWebElement> gubun = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Gubun)).FindElements(By.TagName("input"));
                                IWebElement gubunOption = gubun.FirstOrDefault(x => x.GetAttribute("data-label") == "대변");
                                if (gubunOption != null)
                                {
                                    gubunOption.Click();
                                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t구분 : {0}", "대변"), isSuccessLogHide);
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 {0}회 시도 실패", retryCount));
                            return false;
                        }
                    }

                    // 계정 과목
                    bool vattyn = false;
                    bool trcdty = true;
                    if (slipValue.Account.AccountCode.Length > 0 && copySector.SlipSector.Account.AccountName.Length > 0)
                    {
                        if (slipValue.Account.AccountType.Contains("A") == false)
                        {
                            trcdty = false;
                        }
                        if (slipValue.Account.AccountName.Contains("부가세"))
                        {
                            vattyn = true;
                        }
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement account = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Account.AccountName));
                                if (account.GetAttribute("data-value") != slipValue.Account.GetAccountName())
                                {
                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    account.Click();

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    IWebElement searchType = FindElement(driver, pasteSector.AccountSearchType, FBaseFunc.ElementType.ID);
                                    ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                    foreach (IWebElement option in options)
                                    {
                                        if (option.GetAttribute("value") == "acct_cd")
                                        {
                                            option.Click();
                                            break;
                                        }
                                    }

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    IWebElement searchText = FindElement(driver, pasteSector.AccountSearchText, FBaseFunc.ElementType.ID);
                                    string accountCode = slipValue.Account.AccountCode;
                                    searchText.SendKeys(accountCode);

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    FindElement(driver, pasteSector.AccountSearchBtn, FBaseFunc.ElementType.ID).Click();

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    if (FindElement(driver, pasteSector.AccountSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                    {
                                        FBaseFunc.Ins.SetResultMethod(string.Format("{0} 계정코드를 못 찾았음.", accountCode));
                                        FBaseFunc.Ins.SetLog(string.Format("{0} 계정코드를 못 찾았음.", accountCode));
                                        FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID).Click();
                                    }
                                    else
                                    {
                                        FindElement(driver, pasteSector.AccountSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("acct_nm")).Click();
                                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t계정코드 : {0}", accountCode), isSuccessLogHide);
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙일자
                    if (slipValue.TaxDate.Length > 0 && copySector.SlipSector.IsEmptyTaxDate() == false)
                    {
                        string year = slipValue.TaxDate.Substring(0, 4);
                        int.TryParse(slipValue.TaxDate.Substring(5, 2), out int ret);
                        string month = ret.ToString();
                        int.TryParse(slipValue.TaxDate.Substring(8, 2), out ret);
                        string day = ret.ToString();
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement taxDate = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.TaxDate)).FindElement(By.TagName("input"));
                                taxDate.Click();
                                IWebElement yearSelect = driver.FindElement(By.ClassName("ui-datepicker-year"));
                                ReadOnlyCollection<IWebElement> years = yearSelect.FindElements(By.TagName("option"));
                                IWebElement yearOption = years.FirstOrDefault(x => x.GetAttribute("value").Equals(year));
                                if (yearOption != null)
                                {
                                    yearOption.Click();
                                }
                                
                                IWebElement monthSelect = driver.FindElement(By.ClassName("ui-datepicker-month"));
                                ReadOnlyCollection<IWebElement> months = monthSelect.FindElements(By.TagName("option"));
                                IWebElement monthOption = months.FirstOrDefault(x => x.Text.Contains(month));
                                if (monthOption != null)
                                {
                                    monthOption.Click();
                                }
                                
                                ReadOnlyCollection<IWebElement> days = driver.FindElement(By.ClassName("ui-datepicker-calendar")).FindElements(By.TagName("a"));
                                IWebElement dayOption = days.FirstOrDefault(x => x.Text == day);
                                if (dayOption != null)
                                {
                                    dayOption.Click();
                                }

                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙일자 : {0}", slipValue.TaxDate), isSuccessLogHide);

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙유형
                    if (slipValue.Type.Contains("과세") == false && slipValue.Type.Length > 0 && copySector.SlipSector.IsEmptyType() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement type = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Type)).FindElement(By.TagName("select"));
                                string typeData = type.GetAttribute("data-selectval");
                                if (typeData == null || typeData.Length <= 0)
                                {
                                    typeData = type.FindElement(By.TagName("option")).GetAttribute("value");
                                }

                                if (typeData != slipValue.Type)
                                {
                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    ReadOnlyCollection<IWebElement> types = type.FindElements(By.TagName("option"));
                                    foreach (IWebElement typeName in types)
                                    {
                                        if (slipValue.Type == typeName.GetAttribute("value"))
                                        {
                                            typeName.Click();
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙유형 : {0}", slipValue.Type), isSuccessLogHide);
                                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                            break;
                                        }
                                    }
                                }
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 거래처
                    if (trcdty && slipValue.Correspondent.CorrespondentCode.Length > 0 && copySector.SlipSector.Correspondent.CorrespondentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement correspondent = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Correspondent.CorrespondentName));
                                if (correspondent.GetAttribute("data-value") != slipValue.Correspondent.GetCorrespondentName())
                                {
                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    correspondent.Click();
                                    try
                                    {
                                        bool slideMessage = driver.FindElement(By.Id("go_slideMessage")).Displayed;
                                        if (slideMessage)
                                        {
                                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 현재 계정 {1}은 거래처를 선택할 수 없습니다.", i + 1, slipValue.Account));
                                        }
                                    }
                                    catch
                                    {
                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        IWebElement searchType = FindElement(driver, pasteSector.CorrespondentSearchType, FBaseFunc.ElementType.ID);
                                        ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                        foreach (IWebElement option in options)
                                        {
                                            if (option.GetAttribute("value") == "tr_cd")
                                            {
                                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                                option.Click();
                                                break;
                                            }
                                        }

                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        IWebElement searchText = FindElement(driver, pasteSector.CorrespondentSearchText, FBaseFunc.ElementType.ID);
                                        string correspondentCode = slipValue.Correspondent.CorrespondentCode;
                                        searchText.SendKeys(correspondentCode);

                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        FindElement(driver, pasteSector.CorrespondentSearchBtn, FBaseFunc.ElementType.ID).Click();

                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        if (FindElement(driver, pasteSector.CorrespondentSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                        {
                                            FBaseFunc.Ins.SetResultMethod(string.Format("{0} 거래처코드를 못 찾았음.", correspondentCode));
                                            FBaseFunc.Ins.SetLog(string.Format("{0} 거래처코드를 못 찾았음.", correspondentCode));
                                            FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID).Click();
                                        }
                                        else
                                        {
                                            FindElement(driver, pasteSector.CorrespondentSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("code")).Click();
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t거래처코드 : {0}", correspondentCode), isSuccessLogHide);
                                    }
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 금액
                    if (slipValue.Price.Length > 0 && copySector.SlipSector.IsEmptyPrice() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement price = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Price)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (priceData != slipValue.Price)
                        {
                            price.Clear();
                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                            price.SendKeys(slipValue.Price);
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t금액 : {0}", slipValue.Price), isSuccessLogHide);
                        }
                    }

                    // 적요
                    if (slipValue.Briefs.Length > 0 && copySector.SlipSector.IsEmptyBriefs() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement briefs = slipBodys[i * 2 + 1].FindElement(By.ClassName("summary")).FindElement(By.TagName("input"));
                        string briefsData = briefs.GetAttribute("data-value");
                        if (briefsData != slipValue.Briefs)
                        {
                            briefs.Clear();
                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                            briefs.SendKeys(slipValue.Briefs);
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t적요 : {0}", slipValue.Briefs), isSuccessLogHide);
                        }
                    }

                    // 사용부서
                    if (slipValue.Department.DepartmentCode.Length > 0 && copySector.SlipSector.Department.DepartmentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement department = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.Department.DepartmentName));
                                if (department.GetAttribute("data-code") != slipValue.Department.GetDepartmentName())
                                {
                                    department.Click();
                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);

                                    IWebElement searchType = FindElement(driver, pasteSector.DepartmentSearchType, FBaseFunc.ElementType.ID);
                                    ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                    foreach (IWebElement option in options)
                                    {
                                        if (option.GetAttribute("value") == "dept_cd")
                                        {
                                            option.Click();
                                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                            break;
                                        }
                                    }

                                    IWebElement searchText = FindElement(driver, pasteSector.DepartmentSearchText, FBaseFunc.ElementType.ID);
                                    string departmentCode = slipValue.Department.DepartmentCode;
                                    searchText.SendKeys(departmentCode);

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    FindElement(driver, pasteSector.DepartmentSearchBtn, FBaseFunc.ElementType.ID).Click();

                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    if (FindElement(driver, pasteSector.DepartmentSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                    {
                                        FBaseFunc.Ins.SetResultMethod(string.Format("{0} 부서코드를 못 찾았음.", departmentCode));
                                        FBaseFunc.Ins.SetLog(string.Format("{0} 부서코드를 못 찾았음.", departmentCode));
                                        FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID).Click();
                                    }
                                    else
                                    {
                                        FindElement(driver, pasteSector.DepartmentSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("code")).Click();
                                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t부서코드 : {0}", departmentCode), isSuccessLogHide);
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 공급가액
                    if (vattyn && slipValue.SupplyPrice.Length > 0 && copySector.SlipSector.IsEmptySupplyPrice() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement price = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.SupplyPrice)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (slipValue.SupplyPrice != "0" && priceData != slipValue.SupplyPrice)
                        {
                            try
                            {
                                price.Clear();
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                price.SendKeys(slipValue.SupplyPrice);
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}", slipValue.SupplyPrice), isSuccessLogHide);
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}을 넣을 수 없는 계정입니다.", slipValue.SupplyPrice));
                                FBaseFunc.Ins.SetResultMethod(ex.Message);
                            }
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
        public static bool PasteExpenseReportDirect(EdgeDriver driver)
        {
            if (FBaseFunc.Ins.SelectedList == -1)
            {
                FBaseFunc.Ins.SetResultMethod("선택된 리스트가 없습니다.");

                return false;
            }

            string sendKeys = "arguments[0].value=arguments[1]";
            string setAttribute = "arguments[0].setAttribute(arguments[1], arguments[2])";

            FBaseFunc.CopySectorTable copySector = FBaseFunc.Ins.Cfg.CopySector;
            FBaseFunc.CopyDataTable copyData = FBaseFunc.Ins.CopyedTable[FBaseFunc.Ins.SelectedList];
            FBaseFunc.PasteSectorTable pasteSector = FBaseFunc.Ins.Cfg.PasteSector;

            bool isSuccessLogHide = FBaseFunc.Ins.Cfg.IsSuccessLogHide;
            try
            {
                try
                {
                    IWebElement title = FindElement(driver, copySector.Title, FBaseFunc.ElementType.ID);
                    driver.ExecuteScript(sendKeys, title, copyData.Title);
                }
                catch (Exception ex)
                {
                    FBaseFunc.Ins.SetResultMethod("제목을 찾지 못했습니다.");
                    FBaseFunc.Ins.SetResultMethod(ex.Message);
                    FBaseFunc.Ins.SetLog(ex.Message);
                }
                // 회사
                if (copyData.CompanyValue.CompanyCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement company = FindElement(driver, copySector.CompanySector.CompanyCode, FBaseFunc.ElementType.CLASS);
                    driver.ExecuteScript(setAttribute, company, "data-code", copyData.CompanyValue.CompanyCode);
                    driver.ExecuteScript(setAttribute, company, "data-name", copyData.CompanyValue.CompanyName);
                    driver.ExecuteScript(sendKeys, company.FindElement(By.TagName("input")), copyData.CompanyValue.CompanyName);
                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t회사 : {0}", copyData.CompanyValue.GetCompanyName()), isSuccessLogHide);
                }

                // 사업장
                if (copyData.WorkplaceValue.WorkplaceCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement workplace = FindElement(driver, copySector.WorkplaceSector.WorkplaceCode, FBaseFunc.ElementType.CLASS);
                    driver.ExecuteScript(setAttribute, workplace, "data-code", copyData.WorkplaceValue.WorkplaceCode);
                    driver.ExecuteScript(setAttribute, workplace, "data-name", copyData.WorkplaceValue.WorkplaceName);
                    driver.ExecuteScript(sendKeys, workplace.FindElement(By.TagName("input")), copyData.WorkplaceValue.WorkplaceName);
                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사업장 : {0}", copyData.WorkplaceValue.GetWorkplaceName()), isSuccessLogHide);
                }

                int slipCount = copyData.SlipValues.Count;
                int retryCount = FBaseFunc.Ins.Cfg.RetryCount;
                // 행 추가
                if (FBaseFunc.Ins.Cfg.IsNewFirst)
                {
                    IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                    for (int i = 0; i < slipCount - 1; i++)
                    {
                        try
                        {
                            btnAddRow.Click();
                        }
                        catch (Exception ex)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetLog(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetResultMethod(ex.Message);
                            FBaseFunc.Ins.SetLog(ex.Message);
                        }
                    }
                }

                // 전표 테이블
                ReadOnlyCollection<IWebElement> slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                for (int i = 0; i < slipCount; i++)
                {
                    if (FBaseFunc.Ins.IsTerminateOn)
                    {
                        FBaseFunc.Ins.SetLog("TerminateOn");
                        return false;
                    }

                    bool retRetry = false;
                    // 행 추가
                    if (i >= slipBodys.Count / 2)
                    {
                        IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                btnAddRow.Click();
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t열추가 {0}회 시도 실패", retryCount));
                            return false;
                        }
                        slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                    }

                    FBaseFunc.SlipTable slipValue = copyData.SlipValues[i];

                    // 구분 찾기
                    if (slipValue.Gubun != "차변" && copySector.SlipSector.IsEmptyGubun() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                ReadOnlyCollection<IWebElement> gubun = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Gubun)).FindElements(By.TagName("input"));
                                IWebElement gubunOption = gubun.FirstOrDefault(x => x.GetAttribute("data-label") == "대변");
                                if (gubunOption != null)
                                {
                                    gubunOption.Click();
                                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t구분 : {0}", "대변"), isSuccessLogHide);
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 {0}회 시도 실패", retryCount));
                            return false;
                        }
                    }

                    // 계정 과목
                    bool vattyn = false;
                    bool trcdty = true;
                    if (slipValue.Account.AccountCode.Length > 0 && copySector.SlipSector.Account.AccountName.Length > 0)
                    {
                        if (slipValue.Account.AccountType.Contains("A") == false)
                        {
                            trcdty = false;
                        }
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement account = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Account.AccountName));
                                driver.ExecuteScript(setAttribute, account, "data-trcdty", slipValue.Account.AccountType);
                                if (slipValue.Account.AccountName.Contains("부가세"))
                                {
                                    driver.ExecuteScript(setAttribute, account, "data-vatyn", "Y");
                                    vattyn = true;
                                }
                                else
                                {
                                    driver.ExecuteScript(setAttribute, account, "data-vatyn", "N");
                                }
                                driver.ExecuteScript(setAttribute, account, "data-drcr", slipValue.Account.AccountDebit);
                                driver.ExecuteScript(setAttribute, account, "data-code", slipValue.Account.AccountCode);
                                driver.ExecuteScript(setAttribute, account, "data-name", slipValue.Account.AccountName);
                                driver.ExecuteScript(sendKeys, account.FindElement(By.TagName("input")), slipValue.Account.GetAccountName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t계정 : {0}", slipValue.Account.GetAccountName()), isSuccessLogHide);

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙일자
                    if (slipValue.TaxDate.Length > 0 && copySector.SlipSector.IsEmptyTaxDate() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement taxDate = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.TaxDate)).FindElement(By.TagName("input"));
                                driver.ExecuteScript(sendKeys, taxDate, slipValue.TaxDate);
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙일자 : {0}", slipValue.TaxDate), isSuccessLogHide);
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙유형
                    if (slipValue.Type.Contains("과세") == false && slipValue.Type.Length > 0 && copySector.SlipSector.IsEmptyType() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement type = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Type)).FindElement(By.TagName("select"));
                                string typeData = type.GetAttribute("data-selectval");
                                if (typeData == null || typeData.Length <= 0)
                                {
                                    typeData = type.FindElement(By.TagName("option")).GetAttribute("value");
                                }

                                if (typeData != slipValue.Type)
                                {
                                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                    ReadOnlyCollection<IWebElement> types = type.FindElements(By.TagName("option"));
                                    foreach (IWebElement typeName in types)
                                    {
                                        if (slipValue.Type == typeName.GetAttribute("value"))
                                        {
                                            typeName.Click();
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙유형 : {0}", slipValue.Type), isSuccessLogHide);
                                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                            break;
                                        }
                                    }
                                }
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 거래처
                    if (trcdty && slipValue.Correspondent.CorrespondentCode.Length > 0 && copySector.SlipSector.Correspondent.CorrespondentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement correspondent = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Correspondent.CorrespondentName));
                                driver.ExecuteScript(setAttribute, correspondent, "data-code", slipValue.Correspondent.CorrespondentCode);
                                driver.ExecuteScript(setAttribute, correspondent, "data-name", slipValue.Correspondent.CorrespondentName);
                                driver.ExecuteScript(setAttribute, correspondent, "data-regnb", slipValue.Correspondent.CorrespondentNumber);
                                driver.ExecuteScript(sendKeys, correspondent.FindElement(By.TagName("input")), slipValue.Correspondent.GetCorrespondentName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 : {0}", slipValue.Correspondent.GetCorrespondentName()), isSuccessLogHide);
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 금액
                    if (slipValue.Price.Length > 0 && copySector.SlipSector.IsEmptyPrice() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement price = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Price)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (priceData != slipValue.Price)
                        {
                            if (i == slipCount - 1)
                            {
                                price.SendKeys(slipValue.Price);
                            }
                            else
                            {
                                driver.ExecuteScript(sendKeys, price, slipValue.Price);
                            }
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t금액 : {0}", slipValue.Price), isSuccessLogHide);
                        }
                    }

                    // 적요
                    if (slipValue.Briefs.Length > 0 && copySector.SlipSector.IsEmptyBriefs() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement briefs = slipBodys[i * 2 + 1].FindElement(By.ClassName("summary")).FindElement(By.TagName("input"));
                        string briefsData = briefs.GetAttribute("data-value");
                        if (briefsData != slipValue.Briefs)
                        {
                            driver.ExecuteScript(sendKeys, briefs, slipValue.Briefs);
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t적요 : {0}", slipValue.Briefs), isSuccessLogHide);
                        }
                    }

                    // 사용부서
                    if (slipValue.Department.DepartmentCode.Length > 0 && copySector.SlipSector.Department.DepartmentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement department = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.Department.DepartmentName));
                                driver.ExecuteScript(setAttribute, department, "data-code", slipValue.Department.DepartmentCode);
                                driver.ExecuteScript(setAttribute, department, "data-name", slipValue.Department.DepartmentName);
                                IWebElement departmentInput = department.FindElement(By.TagName("input"));
                                driver.ExecuteScript(setAttribute, departmentInput, "data-code", slipValue.Department.DepartmentCode);
                                driver.ExecuteScript(setAttribute, departmentInput, "data-div-cd", slipValue.Department.WorkplaceCode);
                                driver.ExecuteScript(sendKeys, departmentInput, slipValue.Department.GetDepartmentName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사용부서 : {0}",slipValue.Department.GetDepartmentName()), isSuccessLogHide);

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 공급가액
                    if (vattyn && slipValue.SupplyPrice.Length > 0 && slipValue.SupplyPrice != "0" && copySector.SlipSector.IsEmptySupplyPrice() == false)
                    {
                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                        IWebElement price = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.SupplyPrice)).FindElement(By.TagName("input"));
                        if (i == slipCount - 1)
                        {
                            try
                            {
                                price.SendKeys(slipValue.SupplyPrice);
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}을 넣을 수 없는 계정입니다.", slipValue.SupplyPrice));
                                FBaseFunc.Ins.SetResultMethod(ex.Message);
                            }
                        }
                        else
                        {
                            driver.ExecuteScript(sendKeys, price, slipValue.SupplyPrice);
                        }
                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}", slipValue.SupplyPrice), isSuccessLogHide);
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
        public static bool PasteExpenseReportDefulatSpeedUp(EdgeDriver driver)
        {
            if (FBaseFunc.Ins.SelectedList == -1)
            {
                FBaseFunc.Ins.SetResultMethod("선택된 리스트가 없습니다.");

                return false;
            }

            FBaseFunc.CopySectorTable copySector = FBaseFunc.Ins.Cfg.CopySector;
            FBaseFunc.CopyDataTable copyData = FBaseFunc.Ins.CopyedTable[FBaseFunc.Ins.SelectedList];
            FBaseFunc.PasteSectorTable pasteSector = FBaseFunc.Ins.Cfg.PasteSector;

            string sendKeys = "arguments[0].value=arguments[1]";
            string click = "arguments[0].click()";
            bool isSuccessLogHide = FBaseFunc.Ins.Cfg.IsSuccessLogHide;

            try
            {
                try
                {
                    IWebElement title = FindElement(driver, copySector.Title, FBaseFunc.ElementType.ID);
                    driver.ExecuteScript(sendKeys, title, copyData.Title);
                }
                catch (Exception ex)
                {
                    FBaseFunc.Ins.SetResultMethod("제목을 찾지 못했습니다.");
                    FBaseFunc.Ins.SetResultMethod(ex.Message);
                    FBaseFunc.Ins.SetLog(ex.Message);
                }
                // 회사
                if (copyData.CompanyValue.CompanyCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement company = FindElement(driver, copySector.CompanySector.CompanyCode, FBaseFunc.ElementType.CLASS);
                    if (company.GetAttribute("data-code") != copyData.CompanyValue.CompanyCode)
                    {
                        driver.ExecuteScript(click, company.FindElement(By.TagName("input")));
                        ReadOnlyCollection<IWebElement> bodys = FindElement(driver, pasteSector.Company, FBaseFunc.ElementType.XPATH).FindElements(By.TagName("tr"));
                        bool find = false;
                        foreach (var body in bodys)
                        {
                            if (body.GetAttribute("data-code") == copyData.CompanyValue.CompanyCode)
                            {
//                                driver.ExecuteScript(click, body);
                                body.Click();
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t회사 : {0}", copyData.CompanyValue.CompanyCode), isSuccessLogHide);
                                find = true;
                                break;
                            }
                        }

                        if (find == false)
                        {
                            driver.ExecuteScript(click, FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID));
                            FBaseFunc.Ins.SetResultMethod("사명을 못 찾았습니다.");
                            FBaseFunc.Ins.SetLog("[PasteFlow]\t사명을 못 찾았습니다.");
                            return false;
                        }
                    }
                }

                // 사업장
                if (copyData.WorkplaceValue.WorkplaceCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement workplace = FindElement(driver, copySector.WorkplaceSector.WorkplaceCode, FBaseFunc.ElementType.CLASS);
                    if (workplace.GetAttribute("data-code") != copyData.WorkplaceValue.WorkplaceCode)
                    {
//                        driver.ExecuteScript(click, workplace);
                        workplace.Click();
                        ReadOnlyCollection<IWebElement> bodys = FindElement(driver, pasteSector.Workplace, FBaseFunc.ElementType.XPATH).FindElements(By.TagName("td"));
                        bool find = false;
                        foreach (var body in bodys)
                        {
                            if (body.Text == copyData.WorkplaceValue.WorkplaceCode)
                            {
                                driver.ExecuteScript(click, body);
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사업장 : {0}", copyData.WorkplaceValue.WorkplaceCode), isSuccessLogHide);
                                find = true;
                                break;
                            }
                        }

                        if (find == false)
                        {
                            driver.ExecuteScript(click, FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID));
                            FBaseFunc.Ins.SetResultMethod("사업장을 못 찾았습니다.");
                            FBaseFunc.Ins.SetLog("[PasteFlow]\t사업장을 못 찾았습니다.");
                            return false;
                        }
                    }
                }

                int slipCount = copyData.SlipValues.Count;
                int retryCount = FBaseFunc.Ins.Cfg.RetryCount;
                // 행 추가
                if (FBaseFunc.Ins.Cfg.IsNewFirst)
                {
                    IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                    for (int i = 0; i < slipCount - 1; i++)
                    {
                        try
                        {
                            driver.ExecuteScript(click, btnAddRow);
                        }
                        catch (Exception ex)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetLog(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetResultMethod(ex.Message);
                            FBaseFunc.Ins.SetLog(ex.Message);
                        }
                    }
                }

                // 전표 테이블
                ReadOnlyCollection<IWebElement> slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                for (int i = 0; i < slipCount; i++)
                {
                    if (FBaseFunc.Ins.IsTerminateOn)
                    {
                        FBaseFunc.Ins.SetLog("TerminateOn");
                        return false;
                    }

                    bool retRetry = false;
                    // 행 추가
                    if (i >= slipBodys.Count / 2)
                    {
                        IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                        try
                        {
                            driver.ExecuteScript(click, btnAddRow);
                            retRetry = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetLog(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetResultMethod(ex.Message);
                            FBaseFunc.Ins.SetLog(ex.Message);
                        }

                        slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                    }

                    FBaseFunc.SlipTable slipValue = copyData.SlipValues[i];

                    // 구분 찾기
                    if (slipValue.Gubun != "차변" && copySector.SlipSector.IsEmptyGubun() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                ReadOnlyCollection<IWebElement> gubun = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Gubun)).FindElements(By.TagName("input"));
                                IWebElement gubunOption = gubun.FirstOrDefault(x => x.GetAttribute("data-label") == "대변");
                                if (gubunOption != null)
                                {
                                    gubunOption.Click();
                                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t구분 : {0}", "대변"), isSuccessLogHide);
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 {0}회 시도 실패", retryCount));
                            return false;
                        }
                    }

                    // 계정 과목
                    bool vattyn = false;
                    bool trcdty = true;
                    if (slipValue.Account.AccountCode.Length > 0 && copySector.SlipSector.Account.AccountName.Length > 0)
                    {
                        if (slipValue.Account.AccountType.Contains("A") == false)
                        {
                            trcdty = false;
                        }
                        if (slipValue.Account.AccountName.Contains("부가세"))
                        {
                            vattyn = true;
                        }
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement account = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Account.AccountName));
                                if (account.GetAttribute("data-value") != slipValue.Account.GetAccountName())
                                {
//                                    driver.ExecuteScript(click, account);
                                    account.Click();
                                    IWebElement searchType = FindElement(driver, pasteSector.AccountSearchType, FBaseFunc.ElementType.ID);
                                    ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                    foreach (IWebElement option in options)
                                    {
                                        if (option.GetAttribute("value") == "acct_cd")
                                        {
//                                            driver.ExecuteScript(click, option);
                                            option.Click();
                                            break;
                                        }
                                    }

                                    IWebElement searchText = FindElement(driver, pasteSector.AccountSearchText, FBaseFunc.ElementType.ID);
                                    string accountCode = slipValue.Account.AccountCode;
                                    driver.ExecuteScript(sendKeys, searchText, accountCode);
                                    driver.ExecuteScript(click, FindElement(driver, pasteSector.AccountSearchBtn, FBaseFunc.ElementType.ID));
                                    if (FindElement(driver, pasteSector.AccountSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                    {
                                        FBaseFunc.Ins.SetResultMethod(string.Format("{0} 계정코드를 못 찾았음.", accountCode));
                                        FBaseFunc.Ins.SetLog(string.Format("{0} 계정코드를 못 찾았음.", accountCode));
                                        driver.ExecuteScript(click, FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID));
                                    }
                                    else
                                    {
                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        driver.ExecuteScript(click, FindElement(driver, pasteSector.AccountSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("acct_nm")));
                                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t계정코드 : {0}", accountCode), isSuccessLogHide);
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙일자
                    if (slipValue.TaxDate.Length > 0 && copySector.SlipSector.IsEmptyTaxDate() == false)
                    {
                        string year = slipValue.TaxDate.Substring(0, 4);
                        int.TryParse(slipValue.TaxDate.Substring(5, 2), out int ret);
                        string month = ret.ToString();
                        int.TryParse(slipValue.TaxDate.Substring(8, 2), out ret);
                        string day = ret.ToString();
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement taxDate = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.TaxDate)).FindElement(By.TagName("input"));
//                                driver.ExecuteScript(click, taxDate);
                                taxDate.Click();
                                IWebElement yearSelect = driver.FindElement(By.ClassName("ui-datepicker-year"));
//                                driver.ExecuteScript(click, yearSelect);
                                ReadOnlyCollection<IWebElement> years = yearSelect.FindElements(By.TagName("option"));
                                IWebElement yearOption = years.FirstOrDefault(x => x.GetAttribute("value").Equals(year));
                                if (yearOption != null)
                                {
//                                    driver.ExecuteScript(click, yearOption);
                                    yearOption.Click();
                                }
                                IWebElement monthSelect = driver.FindElement(By.ClassName("ui-datepicker-month"));
//                                driver.ExecuteScript(click, monthSelect);
                                ReadOnlyCollection<IWebElement> months = monthSelect.FindElements(By.TagName("option"));
                                IWebElement monthOption = months.FirstOrDefault(x => x.Text.Contains(month));
                                if (monthOption != null)
                                {
//                                    driver.ExecuteScript(click, monthOption);
                                    monthOption.Click();
                                }
                                ReadOnlyCollection<IWebElement> days = driver.FindElement(By.ClassName("ui-datepicker-calendar")).FindElements(By.TagName("a"));
                                IWebElement dayOption = days.FirstOrDefault(x => x.Text == day);
                                if (dayOption != null)
                                {
                                    driver.ExecuteScript(click, dayOption);
                                }

                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙일자 : {0}", slipValue.TaxDate), isSuccessLogHide);
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙유형
                    if (slipValue.Type.Contains("과세") == false && slipValue.Type.Length > 0 && copySector.SlipSector.IsEmptyType() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement type = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Type)).FindElement(By.TagName("select"));
                                string typeData = type.GetAttribute("data-selectval");
                                if (typeData == null || typeData.Length <= 0)
                                {
                                    typeData = type.FindElement(By.TagName("option")).GetAttribute("value");
                                }

                                if (typeData != slipValue.Type)
                                {
//                                    driver.ExecuteScript(click, type);
                                    ReadOnlyCollection<IWebElement> types = type.FindElements(By.TagName("option"));
                                    foreach (IWebElement typeName in types)
                                    {
                                        if (slipValue.Type == typeName.GetAttribute("value"))
                                        {
//                                            driver.ExecuteScript(click, typeName);
                                            typeName.Click();
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙유형 : {0}", slipValue.Type), isSuccessLogHide);
                                            break;
                                        }
                                    }
                                }
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 거래처
                    if (trcdty && slipValue.Correspondent.CorrespondentCode.Length > 0 && copySector.SlipSector.Correspondent.CorrespondentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement correspondent = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Correspondent.CorrespondentName));
                                if (correspondent.GetAttribute("data-value") != slipValue.Correspondent.GetCorrespondentName())
                                {
//                                    driver.ExecuteScript(click, correspondent);
                                    correspondent.Click();
                                    try
                                    {
                                        bool slideMessage = driver.FindElement(By.Id("go_slideMessage")).Displayed;
                                        if (slideMessage)
                                        {
                                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 현재 계정 {1}은 거래처를 선택할 수 없습니다.", i + 1, slipValue.Account));
                                        }
                                    }
                                    catch
                                    {
                                        IWebElement searchType = FindElement(driver, pasteSector.CorrespondentSearchType, FBaseFunc.ElementType.ID);
                                        ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                        foreach (IWebElement option in options)
                                        {
                                            if (option.GetAttribute("value") == "tr_cd")
                                            {
//                                                driver.ExecuteScript(click, option);
                                                option.Click();
                                                break;
                                            }
                                        }

                                        IWebElement searchText = FindElement(driver, pasteSector.CorrespondentSearchText, FBaseFunc.ElementType.ID);
                                        string correspondentCode = slipValue.Correspondent.CorrespondentCode;
                                        driver.ExecuteScript(sendKeys, searchText, correspondentCode);
                                        driver.ExecuteScript(click, FindElement(driver, pasteSector.CorrespondentSearchBtn, FBaseFunc.ElementType.ID));
                                        if (FindElement(driver, pasteSector.CorrespondentSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                        {
                                            FBaseFunc.Ins.SetResultMethod(string.Format("{0} 거래처코드를 못 찾았음.", correspondentCode));
                                            FBaseFunc.Ins.SetLog(string.Format("{0} 거래처코드를 못 찾았음.", correspondentCode));
                                            driver.ExecuteScript(click, FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID));
                                        }
                                        else
                                        {
                                            Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                            driver.ExecuteScript(click, FindElement(driver, pasteSector.CorrespondentSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("code")));
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t거래처코드 : {0}", correspondentCode), isSuccessLogHide);
                                        }
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 금액
                    if (slipValue.Price.Length > 0 && copySector.SlipSector.IsEmptyPrice() == false)
                    {
                        IWebElement price = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Price)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (priceData != slipValue.Price)
                        {
                            if (i == slipCount - 1)
                            {
                                price.SendKeys(slipValue.Price);
                            }
                            else
                            {
                                driver.ExecuteScript(sendKeys, price, slipValue.Price);
                            }
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t금액 : {0}", slipValue.Price), isSuccessLogHide);
                        }
                    }

                    // 적요
                    if (slipValue.Briefs.Length > 0 && copySector.SlipSector.IsEmptyBriefs() == false)
                    {
                        IWebElement briefs = slipBodys[i * 2 + 1].FindElement(By.ClassName("summary")).FindElement(By.TagName("input"));
                        string briefsData = briefs.GetAttribute("data-value");
                        if (briefsData != slipValue.Briefs)
                        {
                            driver.ExecuteScript(sendKeys, briefs, slipValue.Briefs);
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t적요 : {0}", slipValue.Briefs), isSuccessLogHide);
                        }
                    }

                    // 사용부서
                    if (slipValue.Department.DepartmentCode.Length > 0 && copySector.SlipSector.Department.DepartmentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                IWebElement department = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.Department.DepartmentName));
                                if (department.GetAttribute("data-code") != slipValue.Department.GetDepartmentName())
                                {
                                    department.Click();
                                    IWebElement searchType = FindElement(driver, pasteSector.DepartmentSearchType, FBaseFunc.ElementType.ID);
                                    ReadOnlyCollection<IWebElement> options = searchType.FindElements(By.TagName("option"));
                                    foreach (IWebElement option in options)
                                    {
                                        if (option.GetAttribute("value") == "dept_cd")
                                        {
                                            option.Click();
                                            break;
                                        }
                                    }

                                    IWebElement searchText = FindElement(driver, pasteSector.DepartmentSearchText, FBaseFunc.ElementType.ID);
                                    string departmentCode = slipValue.Department.DepartmentCode;
                                    driver.ExecuteScript(sendKeys, searchText, departmentCode);
                                    driver.ExecuteScript(click, FindElement(driver, pasteSector.DepartmentSearchBtn, FBaseFunc.ElementType.ID));
                                    if (FindElement(driver, pasteSector.DepartmentSearchBody, FBaseFunc.ElementType.XPATH).Text.Contains("검색결과가"))
                                    {
                                        FBaseFunc.Ins.SetResultMethod(string.Format("{0} 부서코드를 못 찾았음.", departmentCode));
                                        FBaseFunc.Ins.SetLog(string.Format("{0} 부서코드를 못 찾았음.", departmentCode));
                                        driver.ExecuteScript(click, FindElement(driver, FBaseFunc.Ins.Cfg.BtnClose, FBaseFunc.ElementType.ID));
                                    }
                                    else
                                    {
                                        Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                        driver.ExecuteScript(click, FindElement(driver, pasteSector.DepartmentSearchBody, FBaseFunc.ElementType.XPATH).FindElement(By.ClassName("code")));
                                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t부서코드 : {0}", departmentCode), isSuccessLogHide);
                                    }
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 공급가액
                    if (vattyn && slipValue.SupplyPrice.Length > 0 && copySector.SlipSector.IsEmptySupplyPrice() == false)
                    {
                        IWebElement price = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.SupplyPrice)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (slipValue.SupplyPrice != "0" && priceData != slipValue.SupplyPrice)
                        {
                            if (i == slipCount - 1)
                            {
                                try
                                {
                                    price.SendKeys(slipValue.SupplyPrice);
                                }
                                catch (Exception ex)
                                {
                                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}을 넣을 수 없는 계정입니다.", slipValue.SupplyPrice));
                                    FBaseFunc.Ins.SetResultMethod(ex.Message);
                                }
                            }
                            else
                            {
                                driver.ExecuteScript(sendKeys, price, slipValue.SupplyPrice);
                            }
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}", slipValue.SupplyPrice), isSuccessLogHide);
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
        public static bool PasteExpenseReportDirectSpeedUp(EdgeDriver driver)
        {
            if (FBaseFunc.Ins.SelectedList == -1)
            {
                FBaseFunc.Ins.SetResultMethod("선택된 리스트가 없습니다.");

                return false;
            }

            string sendKeys = "arguments[0].value=arguments[1]";
            string setAttribute = "arguments[0].setAttribute(arguments[1], arguments[2])";

            FBaseFunc.CopySectorTable copySector = FBaseFunc.Ins.Cfg.CopySector;
            FBaseFunc.CopyDataTable copyData = FBaseFunc.Ins.CopyedTable[FBaseFunc.Ins.SelectedList];
            FBaseFunc.PasteSectorTable pasteSector = FBaseFunc.Ins.Cfg.PasteSector;

            bool isSuccessLogHide = FBaseFunc.Ins.Cfg.IsSuccessLogHide;
            try
            {
                try
                {
                    IWebElement title = FindElement(driver, copySector.Title, FBaseFunc.ElementType.ID);
                    driver.ExecuteScript(sendKeys, title, copyData.Title);
                }
                catch (Exception ex)
                {
                    FBaseFunc.Ins.SetResultMethod("제목을 찾지 못했습니다.");
                    FBaseFunc.Ins.SetResultMethod(ex.Message);
                    FBaseFunc.Ins.SetLog(ex.Message);
                }
                // 회사
                if (copyData.CompanyValue.CompanyCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement company = FindElement(driver, copySector.CompanySector.CompanyCode, FBaseFunc.ElementType.CLASS);
                    driver.ExecuteScript(setAttribute, company, "data-code", copyData.CompanyValue.CompanyCode);
                    driver.ExecuteScript(setAttribute, company, "data-name", copyData.CompanyValue.CompanyName);
                    driver.ExecuteScript(sendKeys, company.FindElement(By.TagName("input")), copyData.CompanyValue.CompanyName);
                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t회사 : {0}", copyData.CompanyValue.GetCompanyName()), isSuccessLogHide);
                }

                // 사업장
                if (copyData.WorkplaceValue.WorkplaceCode.Length > 0)
                {
                    Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                    IWebElement workplace = FindElement(driver, copySector.WorkplaceSector.WorkplaceCode, FBaseFunc.ElementType.CLASS);
                    driver.ExecuteScript(setAttribute, workplace, "data-code", copyData.WorkplaceValue.WorkplaceCode);
                    driver.ExecuteScript(setAttribute, workplace, "data-name", copyData.WorkplaceValue.WorkplaceName);
                    driver.ExecuteScript(sendKeys, workplace.FindElement(By.TagName("input")), copyData.WorkplaceValue.WorkplaceName);
                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사업장 : {0}", copyData.WorkplaceValue.GetWorkplaceName()), isSuccessLogHide);
                }

                int slipCount = copyData.SlipValues.Count;
                int retryCount = FBaseFunc.Ins.Cfg.RetryCount;
                // 행 추가
                if (FBaseFunc.Ins.Cfg.IsNewFirst)
                {
                    IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                    for (int i = 0; i < slipCount - 1; i++)
                    {
                        try
                        {
                            btnAddRow.Click();
                        }
                        catch (Exception ex)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetLog(string.Format("{0}번째 행추가 실패", i + 1));
                            FBaseFunc.Ins.SetResultMethod(ex.Message);
                            FBaseFunc.Ins.SetLog(ex.Message);
                        }
                    }
                }

                // 전표 테이블
                ReadOnlyCollection<IWebElement> slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                for (int i = 0; i < slipCount; i++)
                {
                    if (FBaseFunc.Ins.IsTerminateOn)
                    {
                        FBaseFunc.Ins.SetLog("TerminateOn");
                        return false;
                    }

                    bool retRetry = false;
                    // 행 추가
                    if (i >= slipBodys.Count / 2)
                    {
                        IWebElement btnAddRow = FindElement(driver, FBaseFunc.Ins.Cfg.BtnAddRow, FBaseFunc.ElementType.ID);
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                btnAddRow.Click();
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t행추가 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t열추가 {0}회 시도 실패", retryCount));
                            return false;
                        }
                        slipBodys = FindElement(driver, copySector.Body, FBaseFunc.ElementType.ID).FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                    }

                    FBaseFunc.SlipTable slipValue = copyData.SlipValues[i];

                    // 구분 찾기
                    if (slipValue.Gubun != "차변" && copySector.SlipSector.IsEmptyGubun() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                Thread.Sleep(FBaseFunc.Ins.Cfg.DelayTime);
                                ReadOnlyCollection<IWebElement> gubun = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Gubun)).FindElements(By.TagName("input"));
                                IWebElement gubunOption = gubun.FirstOrDefault(x => x.GetAttribute("data-label") == "대변");
                                if (gubunOption != null)
                                {
                                    gubunOption.Click();
                                    FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t구분 : {0}", "대변"), isSuccessLogHide);
                                }

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t구분 선택 {0}회 시도 실패", retryCount));
                            return false;
                        }
                    }

                    // 계정 과목
                    bool vattyn = false;
                    bool trcdty = true;
                    if (slipValue.Account.AccountCode.Length > 0 && copySector.SlipSector.Account.AccountName.Length > 0)
                    {
                        if (slipValue.Account.AccountType.Contains("A") == false)
                        {
                            trcdty = false;
                        }
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement account = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Account.AccountName));
                                driver.ExecuteScript(setAttribute, account, "data-trcdty", slipValue.Account.AccountType);
                                if (slipValue.Account.AccountName.Contains("부가세"))
                                {
                                    driver.ExecuteScript(setAttribute, account, "data-vatyn", "Y");
                                    vattyn = true;
                                }
                                else
                                {
                                    driver.ExecuteScript(setAttribute, account, "data-vatyn", "N");
                                }
                                driver.ExecuteScript(setAttribute, account, "data-drcr", slipValue.Account.AccountDebit);
                                driver.ExecuteScript(setAttribute, account, "data-code", slipValue.Account.AccountCode);
                                driver.ExecuteScript(setAttribute, account, "data-name", slipValue.Account.AccountName);
                                driver.ExecuteScript(sendKeys, account.FindElement(By.TagName("input")), slipValue.Account.GetAccountName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t계정 : {0}", slipValue.Account.GetAccountName()), isSuccessLogHide);

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t계정 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙일자
                    if (slipValue.TaxDate.Length > 0 && copySector.SlipSector.IsEmptyTaxDate() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement taxDate = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.TaxDate)).FindElement(By.TagName("input"));
                                driver.ExecuteScript(sendKeys, taxDate, slipValue.TaxDate);
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙일자 : {0}", slipValue.TaxDate), isSuccessLogHide);
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙일자 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 증빙유형
                    if (slipValue.Type.Contains("과세") == false && slipValue.Type.Length > 0 && copySector.SlipSector.IsEmptyType() == false)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement type = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Type)).FindElement(By.TagName("select"));
                                string typeData = type.GetAttribute("data-selectval");
                                if (typeData == null || typeData.Length <= 0)
                                {
                                    typeData = type.FindElement(By.TagName("option")).GetAttribute("value");
                                }

                                if (typeData != slipValue.Type)
                                {
                                    ReadOnlyCollection<IWebElement> types = type.FindElements(By.TagName("option"));
                                    foreach (IWebElement typeName in types)
                                    {
                                        if (slipValue.Type == typeName.GetAttribute("value"))
                                        {
                                            typeName.Click();
                                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t증빙유형 : {0}", slipValue.Type), isSuccessLogHide);
                                            break;
                                        }
                                    }
                                }
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t증빙유형 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 거래처
                    if (trcdty && slipValue.Correspondent.CorrespondentCode.Length > 0 && copySector.SlipSector.Correspondent.CorrespondentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement correspondent = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Correspondent.CorrespondentName));
                                driver.ExecuteScript(setAttribute, correspondent, "data-code", slipValue.Correspondent.CorrespondentCode);
                                driver.ExecuteScript(setAttribute, correspondent, "data-name", slipValue.Correspondent.CorrespondentName);
                                driver.ExecuteScript(setAttribute, correspondent, "data-regnb", slipValue.Correspondent.CorrespondentNumber);
                                driver.ExecuteScript(sendKeys, correspondent.FindElement(By.TagName("input")), slipValue.Correspondent.GetCorrespondentName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 : {0}", slipValue.Correspondent.GetCorrespondentName()), isSuccessLogHide);
                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t거래처 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 금액
                    if (slipValue.Price.Length > 0 && copySector.SlipSector.IsEmptyPrice() == false)
                    {
                        IWebElement price = slipBodys[i * 2].FindElement(By.ClassName(copySector.SlipSector.Price)).FindElement(By.TagName("input"));
                        string priceData = price.GetAttribute("data-value");
                        if (i == slipCount - 1)
                        {
                            price.SendKeys(slipValue.Price);
                        }
                        else
                        {
                            driver.ExecuteScript(sendKeys, price, slipValue.Price);
                        }
                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t금액 : {0}", slipValue.Price), isSuccessLogHide);
                    }

                    // 적요
                    if (slipValue.Briefs.Length > 0 && copySector.SlipSector.IsEmptyBriefs() == false)
                    {
                        IWebElement briefs = slipBodys[i * 2 + 1].FindElement(By.ClassName("summary")).FindElement(By.TagName("input"));
                        string briefsData = briefs.GetAttribute("data-value");
                        if (briefsData != slipValue.Briefs)
                        {
                            driver.ExecuteScript(sendKeys, briefs, slipValue.Briefs);
                            FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t적요 : {0}", slipValue.Briefs), isSuccessLogHide);
                        }
                    }

                    // 사용부서
                    if (slipValue.Department.DepartmentCode.Length > 0 && copySector.SlipSector.Department.DepartmentName.Length > 0)
                    {
                        retRetry = false;
                        for (int retry = 0; retry < retryCount; retry++)
                        {
                            try
                            {
                                IWebElement department = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.Department.DepartmentName));
                                driver.ExecuteScript(setAttribute, department, "data-code", slipValue.Department.DepartmentCode);
                                driver.ExecuteScript(setAttribute, department, "data-name", slipValue.Department.DepartmentName);
                                IWebElement departmentInput = department.FindElement(By.TagName("input"));
                                driver.ExecuteScript(setAttribute, departmentInput, "data-code", slipValue.Department.DepartmentCode);
                                driver.ExecuteScript(setAttribute, departmentInput, "data-div-cd", slipValue.Department.WorkplaceCode);
                                driver.ExecuteScript(sendKeys, departmentInput, slipValue.Department.GetDepartmentName());
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t사용부서 : {0}", slipValue.Department.GetDepartmentName()), isSuccessLogHide);

                                retRetry = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetLog(ex.Message);
                                FBaseFunc.Ins.SetLog(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                                FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 fail count {0}", retry + 1));
                            }
                        }

                        if (retRetry == false)
                        {
                            FBaseFunc.Ins.SetResultMethod(string.Format("[ERR]\t부서 선택 {0}회 시도 실패", retryCount));
                        }
                    }

                    // 공급가액
                    if (vattyn && slipValue.SupplyPrice.Length > 0 && slipValue.SupplyPrice != "0" && copySector.SlipSector.IsEmptySupplyPrice() == false)
                    {
                        IWebElement price = slipBodys[i * 2 + 1].FindElement(By.ClassName(copySector.SlipSector.SupplyPrice)).FindElement(By.TagName("input"));
                        if (i == slipCount - 1)
                        {
                            try
                            {
                                price.SendKeys(slipValue.SupplyPrice);
                            }
                            catch (Exception ex)
                            {
                                FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}을 넣을 수 없는 계정입니다.", slipValue.SupplyPrice));
                                FBaseFunc.Ins.SetResultMethod(ex.Message);
                            }
                        }
                        else
                        {
                            driver.ExecuteScript(sendKeys, price, slipValue.SupplyPrice);
                        }
                        FBaseFunc.Ins.SetResultMethod(string.Format("[Paste]\t공급가액 : {0}", slipValue.SupplyPrice), isSuccessLogHide);
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
        private static ReadOnlyCollection<IWebElement> FindElements(EdgeDriver driver, string data, FBaseFunc.ElementType type, int timeout = 0)
        {
            if (timeout == 0)
            {
                timeout = FBaseFunc.Ins.Cfg.Timeout;
            }

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(timeout));
            return type switch
            {
                FBaseFunc.ElementType.XPATH => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath(data))),
                FBaseFunc.ElementType.ID => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id(data))),
                FBaseFunc.ElementType.CLASS => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.ClassName(data))),
                _ => null,
            };
        }
    }
}
