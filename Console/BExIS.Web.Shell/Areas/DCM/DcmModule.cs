﻿using BExIS.Modules.Dcm.UI.Helpers;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI
{
    public class DcmModule : ModuleBase
    {
        public DcmModule() : base("dcm")
        {
            LoggerFactory.GetFileLogger().LogCustom("...ctor of dcm...");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("...start install of dim...");
            try
            {
                base.Install();
                DcmSeedDataGenerator.GenerateSeedData();
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("...end install of dim...");
        }
    }
}
