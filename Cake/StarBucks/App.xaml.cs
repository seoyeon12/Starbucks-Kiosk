﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StarBucks
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SeatDataSource SeatData = new SeatDataSource();
        public static DrinkDataSource DrinkData = new DrinkDataSource();
        public static analytics analytics = new analytics();
    }
}
