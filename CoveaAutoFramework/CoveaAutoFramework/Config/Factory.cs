using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Ninject;
using Ninject.Extensions.Conventions;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using CoveaFramework;
using CoveaFramework.ApiClients;
//using ProvidentTestProject.Pages;

namespace CoveaFramework.Config
{
    public class Factory : NinjectModule
    {
        private static IKernel kernel;
        private static Factory instance;

        private Factory()
        {
        }

        public static Factory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Factory();
                    // initialise static ninject kernel
                    kernel = new StandardKernel(instance);
                }
                return instance;
            }
        }

        public T Get<T>()
        {
            return kernel.Get<T>();
        }

        public void Inject(object inject)
        {
            kernel.Inject(inject);
        }


        public override void Load()
        {
            // zapi client
            Bind<IZapi>().ToConstant(
                new JiraHttpClient(Settings.JiraUrl, Settings.JiraUser, Settings.JiraPassword));

             //BindPageModels();
            //BindWebDriver();
            //bindDataModel();

        }

       
    }

}
