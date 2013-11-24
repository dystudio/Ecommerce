﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Web.Apps.Ecommerce.Helpers;
using MrCMS.Web.Apps.Ecommerce.Models;

namespace MrCMS.Web.Apps.Ecommerce.Services.Reports
{
    public interface IChartService
    {
        void SetBarChartLabelsAndData(ref ChartModel model, IEnumerable<IList<KeyValuePair<string, decimal>>> items);
        void SetLineChartData(ref ChartModel model, IEnumerable<IList<KeyValuePair<DateTime, decimal>>> items);
        void SetLineChartLabels(ref ChartModel model);
    }

    public class ChartService : IChartService
    {
        public void SetBarChartLabelsAndData(ref ChartModel model, IEnumerable<IList<KeyValuePair<string, decimal>>> items)
        {
            model.Labels = new List<string>();
            model.MultipleData = new Dictionary<string, List<decimal>>();

            foreach (var label in items.SelectMany(item => item.Select(x => x.Key).Distinct()))
            {
                if(model.Labels.All(x => x != label))
                    model.Labels.Add(label);
            }

            foreach (var item in items)
            {
                var data= model.Labels.Select(label => item.Any(x => x.Key == label) ? item.SingleOrDefault(x => x.Key == label).Value : 0).ToList();
                model.MultipleData.Add(model.MultipleData.Count.ToString(), data.Any(x => x != 0)?data:new List<decimal>());
            }
        }
        public void SetLineChartData(ref ChartModel model, IEnumerable<IList<KeyValuePair<DateTime, decimal>>> items)
        {
            foreach (var item in items)
            {
                var data = new List<decimal>();
                var ts = model.To - model.From;
                var oldDate = DateTime.Parse(model.From.Date.ToString());
                var currentDate = oldDate;

                if (ts.Days == 1)
                {
                    for (var i = 0; i < 24; i++)
                    {
                        data.Add(item.Where(x => x.Key.Hour == i+1).Sum(x => x.Value));
                    }
                }
                else if (model.From.Month == model.To.Month || ts.Days < 31)
                {
                    for (var i = 0; i < ts.Days; i++)
                    {
                        oldDate = currentDate;
                        currentDate = oldDate.AddDays(1);
                        data.Add(i == 0
                                     ? item.Where(x => x.Key.Date == currentDate.Date).Sum(x => x.Value)
                                     : item.Where(x => oldDate.Date <= x.Key && x.Key < currentDate.Date)
                                            .Sum(x => x.Value));
                    }
                }
                else
                {
                    while (oldDate.Month <= currentDate.Month)
                    {
                        oldDate = currentDate;
                        data.Add(item.Where(x => x.Key.Month == currentDate.Month).Sum(x => x.Value));
                        currentDate = oldDate.AddMonths(1);
                    }
                }

                model.MultipleData.Add(model.MultipleData.Count.ToString(), data.Any(x => x != 0) ? data : new List<decimal>());
            }
        }

        public void SetLineChartLabels(ref ChartModel model)
        {
            model.Labels = new List<string>();
            var ts = model.To - model.From;
            var oldDate = DateTime.Parse(model.From.Date.ToString());
            var currentDate = oldDate;

            if (ts.Days == 1)
            {
                for (var i = 0; i < 24; i++)
                {
                    oldDate = currentDate;
                    currentDate = oldDate.AddHours(1);
                    model.Labels.Add(oldDate.Hour.ToString());
                }
            }
            else if (model.From.Month == model.To.Month || ts.Days<31)
            {
                for (var i = 0; i < ts.Days; i++)
                {
                    oldDate = currentDate;
                    currentDate = oldDate.AddDays(1);
                    model.Labels.Add(oldDate.ToString("dd/MM"));
                }
            }
            else
            {
                while (oldDate.Month <= currentDate.Month)
                {
                    oldDate = currentDate;
                    currentDate = oldDate.AddMonths(1);
                    model.Labels.Add(oldDate.Month.GetMonth());
                }
            }
        }
    }
}