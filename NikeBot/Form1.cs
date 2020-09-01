using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System.Diagnostics;

namespace NikeBot
{
    public partial class Form1 : Form
    {
        readonly By loginBtnXPath = By.XPath("//*[@class='join-log-in text-color-grey prl3-sm pt2-sm pb2-sm fs12-sm d-sm-b']");
        readonly By emailInputXPath = By.Name("emailAddress");//By.XPath("//*[@name='emailAddress']");
        readonly By passwordInputXPath = By.Name("password");//By.XPath("//*[@name='password']");
        readonly By submitLoginXPath = By.XPath("//*[@class='nike-unite-submit-button loginSubmit nike-unite-component']/input");
        readonly By errorPanelXPath = By.Id("nike-unite-error-view");//By.XPath("//*[@id='nike-unite-error-view']");
        readonly By loginPanelXPath = By.XPath("//*[@class='js-modal modal ']");
        readonly By dissmisPanelXPath = By.XPath("//*[@value='Dismiss this error']");


        readonly By buyDivXPath = By.XPath("//*[@class='buying-tools-container']");
        readonly By buyBtnXPath = By.XPath("//*[@class='buying-tools-container ']/div/button");
        private By sizeXPath()
        {
            return By.XPath("//*[text()='EU " + sizeTextBox.Text +"']");
        }
        readonly By cartXPath = By.XPath("//*[@aria-label='Bag']");
        readonly By checkoutBtnXPath = By.XPath("//*[@data-automation='member-checkout-button']");
        readonly By consentCheckboxXPath = By.XPath("//*[@class='checkbox-container']/input");
        readonly By consentCheckboxParentXPath = By.XPath("//*[@class='checkbox-container']");
        readonly By consentCheckboxParentParentXPath = By.XPath("//*[@class='checkbox-container']/..");
        readonly By finishShippingBtnXPath = By.Id("shippingSubmit");//By.XPath("//*[@id='shippingSubmit']");
        readonly By finishBillingBtnXPath = By.Id("billingSubmit");//By.XPath("//*[@id='billingSubmit']");
        readonly By nameOnCardXPath = By.Id("CreditCardHolder"); //By.XPath("//*[@id='CreditCardHolder']");
        readonly By cardNumberXPath = By.Id("KKnr"); //By.XPath("//*[@id='KKnr']");
        readonly By securityCodeXPath = By.Id("CCCVC"); //By.XPath("//*[@id='CCCVC']");
        readonly By paymentIFrameXPath = By.Id("paymentIFrame"); //By.XPath("//*[@id='paymentIFrame']");
        readonly By btnPurchaseXPath = By.Id("BtnPurchase"); 
        private By cardMonth(int nr)
        {
            return By.XPath("//*[@id='KKMonth']/option[@value=" + nr + "]");
        }
        private By cardYear(int nr)
        {
            return By.XPath("//*[@id='KKYear']/option[@value=" + nr + "]");
        }
        readonly string testShoe = "https://www.nike.com/ro/launch/t/ispa-overreact-sandal-wheat";
        readonly string realShoe = "https://www.nike.com/ro/launch/t/sb-dunk-low-medicom-be-rbrick1";

        IWebDriver driver;
        Actions builder;
        WebDriverWait wait, shortWait;

        public Form1()
        {
            InitializeComponent();
        }

        private void openChromeBtn_Click(object sender, EventArgs e)
        {
            startThisBot(realShoe);
        }

        private void startThisBot(string shoeUrl)
        {
            string email = emailTextBox.Text;
            string password = passwordTextBox.Text;
            string nameOnCard = nameOnCardTextBox.Text;
            string cardNumber = cardNumberTextBox.Text;
            string expirationMonthString = expirationMonthTextBox.Text;
            string expirationYearString = expirationYearTextBox.Text;
            string securityCode = securityCodeTextBox.Text;

            if (!Verify(new List<string>{email, password, nameOnCard, cardNumber, expirationMonthString, expirationYearString, securityCode}))
            {
                MessageBox.Show("Please input valid data in all fields");
                return;
            }

            int expirationMonth, expirationYear;
            try
            {
                expirationMonth = int.Parse(expirationMonthString);
                expirationYear = int.Parse(expirationYearString);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Expiration month and year must be numbers");
                return;
            }

            StartBrowser(shoeUrl);

            Login(email, password);

            RefreshUntilBuy();

            Cart();

            Checkout(nameOnCard, cardNumber, expirationMonth, expirationYear, securityCode);
        }

        void StartBrowser(string shoeUrl)
        {
            int sWait = 5, lWait = 10;
            try
            {
                sWait = int.Parse(shortWaitTextBox.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("ShortWait must be numbers");
                return;
            }
            try
            {
                lWait = int.Parse(longWaitTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("LongWait must be numbers");
                return;
            }
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            driver = new ChromeDriver(Application.StartupPath + @"\ChromeDrivers\", options);
            builder = new Actions(driver);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, lWait));
            shortWait = new WebDriverWait(driver, new TimeSpan(0, 0, sWait));
            driver.Manage().Window.Maximize();
            driver.Url = shoeUrl;

            System.Diagnostics.Debug.WriteLine("Finished StartBrowser");
        }

        void Login(string email, string password)
        {
            CanGetElement(loginBtnXPath, wait);
            retryingFindClick(loginBtnXPath);

            CanGetElement(emailInputXPath, wait);
            retryingFindSendKeys(emailInputXPath, email);

            while (!LoginTryAgain(password)) ;

            System.Diagnostics.Debug.WriteLine("Finished Login");
        }

        bool LoginTryAgain(string password)
        {
            CanGetElement(passwordInputXPath, wait);
            retryingFindSendKeys(passwordInputXPath, password);

            CanGetElement(submitLoginXPath, wait);
            retryingFindClick(submitLoginXPath);

            wait.Until(driver =>
            {
                return retryingFindGetAttribute(loginPanelXPath, "class") == "js-modal modal " || retryingFindGetAttribute(errorPanelXPath, "style") == "display: block;";
            });
            if (retryingFindGetAttribute(loginPanelXPath, "class") != "js-modal modal ")
            {
                CanGetElement(dissmisPanelXPath, wait);
                retryingFindClick(dissmisPanelXPath);
                return false;
            }
            return true;
        }

        void RefreshUntilBuy()
        {
            while(true)
            {
                IWebElement buyButton = null, comingSoon = null;
                shortWait.Until(driver =>
                {
                    try
                    {
                        buyButton = driver.FindElement(buyBtnXPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("SearchBuy" + ex.Message);
                    }
                    try
                    {
                        comingSoon = driver.FindElement(buyDivXPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("SearchComing" + ex.Message);
                    }
                    return buyButton != null || comingSoon != null;
                });
                if(buyButton != null)
                {
                    CanGetElement(sizeXPath(), wait);
                    retryingFindClick(sizeXPath());
                    retryingFindSendKeys(buyBtnXPath, "s");
                    CanGetElement(buyBtnXPath, wait);
                    retryingFindClick(buyBtnXPath);

                    CanGetElement(cartXPath, wait);
                    retryingFindClick(cartXPath);
                    System.Diagnostics.Debug.WriteLine("Finished RefreshUntilBuy");
                    return;
                }
                driver.Navigate().Refresh();
            }
        }

        void Cart()
        {
            CanGetElement(checkoutBtnXPath, wait);
            retryingFindClick(checkoutBtnXPath);
            try
            {
                shortWait.Until(driver =>
                {
                    Debug.WriteLine(driver.PageSource.Substring(10, 20));
                    if (!driver.PageSource.Contains("class=\" async - hide\"") && driver.Title != "Nike Store. Cart.")
                        return true;
                    return false;
                });
            }
            catch (Exception ex)
            {
                driver.Navigate().Refresh();
                Cart();
                return;
            }

            System.Diagnostics.Debug.WriteLine("Finished Cart");
        }

        void Checkout(string nameOnCard, string cardNumber, int expirationMonth, int expirationYear, string securityCode)
        {
            CanGetElement(consentCheckboxParentXPath, wait);
            IWebElement Indicator = CanGetElement(consentCheckboxParentParentXPath, wait);
            IJavaScriptExecutor exec = (IJavaScriptExecutor)driver;
            exec.ExecuteScript("arguments[0].click();", driver.FindElement(consentCheckboxXPath));
            CanGetElement(finishShippingBtnXPath, wait);
            retryingFindClick(finishShippingBtnXPath);

            if (Indicator.GetAttribute("class") == "gdpr-inner-section is-invalid-gdpr")
            {
                exec.ExecuteScript("arguments[0].click();", driver.FindElement(consentCheckboxXPath));
                CanGetElement(finishShippingBtnXPath, wait);
                retryingFindClick(finishShippingBtnXPath);
            }

            
            CanGetElement(finishBillingBtnXPath, wait);
            retryingFindClick(finishBillingBtnXPath);

            CanGetElement(paymentIFrameXPath, wait);
            driver.SwitchTo().Frame(driver.FindElement(paymentIFrameXPath));

            CanGetElement(nameOnCardXPath, wait);
            retryingFindSendKeys(nameOnCardXPath, nameOnCard);
            retryingFindSendKeys(cardNumberXPath, cardNumber.Substring(0, 4));
            retryingFindSendKeys(cardNumberXPath, cardNumber.Substring(4, 4));
            retryingFindSendKeys(cardNumberXPath, cardNumber.Substring(8, 4));
            retryingFindSendKeys(cardNumberXPath, cardNumber.Substring(12, 4));
            
            /*foreach(char c in cardNumber.ToCharArray())
            {
                retryingFindSendKeys(cardNumberXPath, c + "");
            }*/
            //retryingFindSendKeys(cardNumberXPath, cardNumber);
            retryingFindClick(cardMonth(expirationMonth));
            retryingFindClick(cardYear(expirationYear));
            retryingFindSendKeys(securityCodeXPath, securityCode);
            retryingFindClick(btnPurchaseXPath);

            driver.SwitchTo().DefaultContent();

            System.Diagnostics.Debug.WriteLine("Finished Checkout");
        }

        public bool retryingFindClick(By by)
        {
            bool result = false;
            int attempts = 0;
            while (attempts < 15)
            {
                try
                {
                    driver.FindElement(by).Click();
                    result = true;
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("retryingFindGetAttribute: " + by + ", " + ex.Message);
                }
                attempts++;
            }
            return result;
        }

        public string retryingFindGetAttribute(By by, string attribute)
        {
            int attempts = 0;
            while (attempts < 2)
            {
                try
                {
                    return driver.FindElement(by).GetAttribute(attribute);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("retryingFindClick: " + by + ", " + ex.Message);
                }
                attempts++;
            }
            return null;
        }

        public bool retryingFindSendKeys(By by, string keys)
        {
            bool result = false;
            int attempts = 0;
            while (attempts < 6)
            {
                try
                {
                    driver.FindElement(by).SendKeys(keys);
                    result = true;
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("retryingFindSendKeys: " + ex.Message);
                }
                attempts++;
            }
            return result;
        }
        
        IWebElement CanGetElement(By what, WebDriverWait w, bool checkEnabledDisplayed = true)
        {
            try
            {
                w.Until(driver =>
                {
                    try
                    {
                        IWebElement e = driver.FindElement(what);
                        if (checkEnabledDisplayed)
                            return e != null && e.Displayed && e.Enabled;
                        else
                            return e != null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("CanGetElement" + ex.Message);
                    }
                    return false;
                });
                return driver.FindElement(what);
            }
            catch (Exception ex)
            { }
            return null;
        }

        bool Verify(List<string> allStrings)
        {
            foreach(string str in allStrings)
            {
                if(!VerifyString(str))
                {
                    return false;
                }
            }
            return true;
        }

        bool VerifyString(string what)
        {
            if (what == null || what == "")
            {
                return false;
            }
            return true;
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            startThisBot(testShoe);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //driver?.Close();
        }
    }
}
