﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Io.Transform.Input;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class SubmitValidationController : Controller
    {
        private TaskManager TaskManager;
        private FileStream Stream;
        //
        // GET: /DCM/Validation/

        [HttpGet]
        public ActionResult Validation(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);
                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());

                // set valid to false if exist
                if (TaskManager.Bus.ContainsKey(TaskManager.VALID))
                {
                    TaskManager.Bus.Remove(TaskManager.VALID);
                }
            }

            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Validation(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();


            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey(TaskManager.VALID))
                {
                    bool valid = Convert.ToBoolean(TaskManager.Bus[TaskManager.VALID]);

                    if (valid)
                    {
                        TaskManager.Current().SetValid(true);
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Not Valid."));
                    }

                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Validation failed! Please check that the information you provided  in the previous steps conforms to your data file."));
                    model.StepInfo = TaskManager.Current();
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            model.StepInfo = TaskManager.Current();

            return PartialView(model);
        }


        [HttpPost]
        public ActionResult ValidateFile()
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                try
                {
                    long id = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                    DataStructureManager dsm = new DataStructureManager();
                    long iddsd = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                    {
                        // open file
                        ExcelReader reader = new ExcelReader();
                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                        reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), sds, id);
                        model.ErrorList = reader.errorMessages;
                    }

                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") ||
                        TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                    {
                        AsciiReader reader = new AsciiReader();
                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                        reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], sds, id);
                        model.ErrorList = reader.errorMessages;
                    }
                }
                catch (Exception ex)
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid. :  " + ex.Message));
                    TaskManager.AddToBus(TaskManager.VALID, false);

                }
                finally
                {
                    Stream.Close();
                }
            }
            else
            {
                model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
                TaskManager.AddToBus(TaskManager.VALID, false);
            }

            if (model.ErrorList.Count() == 0)
            {
                model.Validated = true;
                TaskManager.AddToBus(TaskManager.VALID, true);
            }

            return PartialView(TaskManager.Current().GetActionInfo.ActionName, model);
        }
    }
}