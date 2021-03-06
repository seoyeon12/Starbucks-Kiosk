﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using StarBucks.Analytics;

namespace StarBucks
{
    public partial class analytics : Window
    {
        #region Valuable

        // Starbucks.Analytics 인스턴스
        statics stat;

        // Background Worker 완료 유무를 확인하기 위한 변수
        private Boolean menuBackgroundWorkerFinished = false;
        private Boolean todayBackgroundWorkerFinished = false;
        private Boolean categoryBackgroundWorkerFinished = false;
        private int todayAmount = 0;
        private bool connectedToServer = true;
        private notify notify;
        #endregion

        public analytics()
        {
            InitializeComponent();

            // StarBucks.Analytics 인스턴스 초기화
            stat = new statics();

            // 차트 초기 설정
            InitBaseChart();

            if(App.socketController != null)
            {
                var sockInstance = App.socketController.GetSocketInstance();
                App.socketController.lostEvent += SocketController_lostEvent;
                App.socketController.connectEvent += SocketController_connectEvent;

                notify = new notify(this, ToastNotifications.Position.Corner.BottomCenter);

                if (sockInstance.Connected)
                {
                    this.sendTodayAmount.IsEnabled = true;
                }
                else
                {
                    // 오프라인 모드
                    this.sendTodayAmount.IsEnabled = false;
                }
            }
            else
            {
                // 오프라인 모드
                this.sendTodayAmount.IsEnabled = false;
            }
        }

        private void SocketController_connectEvent(object sender, Network.connectedArgs e)
        {
            if(e.connected == true)
            {
                App.Current.Dispatcher.Invoke(new Action(delegate
                {
                    notify.Send_Reconnected_Message();
                    this.sendTodayAmount.IsEnabled = true;
                    connectedToServer = true;
                }));
            }
            else
            {
                var socketInstance = App.socketController?.GetSocketInstance();
                if (socketInstance.reconnectAttempt > 10)
                {
                    App.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        connectedToServer = false;
                        this.sendTodayAmount.IsEnabled = false;
                    }));
                }
            }
        }

        private void SocketController_lostEvent(object sender, Network.lostConnectionArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(delegate
            {
                notify.Send_Disconnected_Message();
                this.sendTodayAmount.IsEnabled = false;
                connectedToServer = false;
            }));
        }

        #region Statics

        private DataSet GetMenuStatics()
        {
            DataSet ds = stat.GetPayments();

            ds.Tables[0].Columns.Add("paymentMethodText", Type.GetType("System.String"));

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                for (var i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows.IndexOf(row) != i && ds.Tables[0].Rows[i].RowState != DataRowState.Deleted)
                    {
                        try
                        {
                            if (row["paymentMethod"].ToString() == "CASH")
                            {
                                row["paymentMethodText"] = "현금";
                            }
                            else
                            {
                                row["paymentMethodText"] = "카드";
                            }

                            if (ds.Tables[0].Rows[i]["paymentMethod"].ToString() == "CASH")
                            {
                                ds.Tables[0].Rows[i]["paymentMethodText"] = "현금";
                            }
                            else
                            {
                                ds.Tables[0].Rows[i]["paymentMethodText"] = "카드";
                            }

                            if (ds.Tables[0].Rows[i]["paymentfor"].ToString() == row["paymentfor"].ToString() && ds.Tables[0].Rows[i]["paymentMethod"].ToString() == row["paymentMethod"].ToString())
                            {
                                ds.Tables[0].Rows[i]["paymentAmount"] = Convert.ToInt64(ds.Tables[0].Rows[i]["paymentAmount"]) + Convert.ToInt64(row["paymentAmount"]);
                                row.Delete();

                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            ds.AcceptChanges();

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.menu.ItemsSource = ds.Tables[0].DefaultView;
            }));

            return ds;
        }

        private DataSet GetTodayStatics()
        {
            DataSet ds = stat.GetPayments();

            ds.Tables[0].Columns.Add("paymentMethodText", Type.GetType("System.String"));

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    if (row["paymentMethod"].ToString() == "CASH")
                    {
                        row["paymentMethodText"] = "현금";
                    }
                    else
                    {
                        row["paymentMethodText"] = "카드";
                    }

                    if (DateTime.Today.Date.CompareTo(DateTime.Parse(row["paymentDate"].ToString()).Date) != 0)
                    {
                        row.Delete();
                    }
                    else
                    {
                        todayAmount += Convert.ToInt32(row["paymentAmount"]);
                    }
                }
            }

            ds.AcceptChanges();

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.todayAmountLabel.Content = "오늘 판매량: " + todayAmount + " 원";
                this.today.ItemsSource = ds.Tables[0].DefaultView;
            }));

            return ds;
        }

        private DataSet GetCategoryStatics()
        {
            DataSet ds = stat.GetPayments();

            ds.Tables[0].Columns.Add("paymentMethodText", Type.GetType("System.String"));

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                for (var i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows.IndexOf(row) != i && ds.Tables[0].Rows[i].RowState != DataRowState.Deleted)
                    {
                        try
                        {
                            if (row["paymentMethod"].ToString() == "CASH")
                            {
                                row["paymentMethodText"] = "현금";
                            }
                            else
                            {
                                row["paymentMethodText"] = "카드";
                            }

                            if (ds.Tables[0].Rows[i]["paymentMethod"].ToString() == "CASH")
                            {
                                ds.Tables[0].Rows[i]["paymentMethodText"] = "현금";
                            }
                            else
                            {
                                ds.Tables[0].Rows[i]["paymentMethodText"] = "카드";
                            }

                            if (ds.Tables[0].Rows[i]["category"].ToString() == row["category"].ToString() && ds.Tables[0].Rows[i]["paymentMethod"].ToString() == row["paymentMethod"].ToString())
                            {
                                ds.Tables[0].Rows[i]["paymentAmount"] = Convert.ToInt64(ds.Tables[0].Rows[i]["paymentAmount"]) + Convert.ToInt64(row["paymentAmount"]);
                                row.Delete();
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            ds.AcceptChanges();

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.category.ItemsSource = ds.Tables[0].DefaultView;
            }));

            return ds;
        }

        #endregion

        #region Charts

        private void InitBaseChart()
        {
            var todayCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "판매량",
                    Values = null
                },
                new LineSeries
                {
                    Title = "카드",
                    Values = null
                },
                new LineSeries
                {
                    Title = "현금",
                    Values = null
                },
            };

            var menuCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "판매량",
                    Values = new ChartValues<double>{ },
                },
            };

            var categoryCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "판매량",
                    Values = new ChartValues<double>{ },
                },
            };

            this.today_chart.Series = todayCollection;
            this.menu_chart.Series = menuCollection;
            this.category_chart.Series = categoryCollection;
        }

        private void InitTodayChart(DataSet ds)
        {
            var timeIdxCount = new int[24];
            var cardIdxCount = new int[24];
            var cashIdxCount = new int[24];

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                try
                {
                    var timeIdx = DateTime.Parse(row["paymentDate"].ToString()).Hour;
                    if (row["paymentMethod"].ToString() == "CASH")
                    {
                        cashIdxCount[timeIdx] = cashIdxCount[timeIdx] + 1;
                    }
                    else
                    {
                        cardIdxCount[timeIdx] = cardIdxCount[timeIdx] + 1;
                    }

                    timeIdxCount[timeIdx] = timeIdxCount[timeIdx] + 1;
                }
                catch
                {

                }
            }

            var Labels = new[] { "00 시", "01 시", "02 시", "03 시", "04 시", "05 시", "06 시", "07 시", "08 시", "09 시", "10 시", "11 시", "12 시",
                "13 시", "14 시", "15 시", "16 시", "17 시", "18 시", "19 시", "20 시", "21 시", "22 시", "23 시", "24 시" };

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.today_chart.Series[0].Values = new ChartValues<int>(timeIdxCount);
                this.today_chart.Series[1].Values = new ChartValues<int>(cardIdxCount);
                this.today_chart.Series[2].Values = new ChartValues<int>(cashIdxCount);
                this.today_chart_X.Labels = Labels;
            }));
        }

        private void InitMenuChart(DataSet ds)
        {
            var tempValue = new ChartValues<double> { };

            var LabelsList = new List<String>();

            for (var i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                try
                {
                    LabelsList.Add(ds.Tables[0].Rows[i]["paymentfor"].ToString() + "(" + ds.Tables[0].Rows[i]["paymentMethodText"].ToString() + ")");
                    tempValue.Add(Convert.ToDouble(ds.Tables[0].Rows[i]["paymentAmount"].ToString()));
                }
                catch
                {

                }
            }

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.menu_chart.Series[0].Values = tempValue;
                this.menu_chart.AxisY[0].Labels = LabelsList.ToArray();
            }));
        }

        private void InitCategoryChart(DataSet ds)
        {
            var tempValues = new ChartValues<double> { };
            var LabelsList = new List<String>();

            for (var i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                try
                {
                    LabelsList.Add(ds.Tables[0].Rows[i]["category"].ToString() + "(" + ds.Tables[0].Rows[i]["paymentMethodText"].ToString() + ")");
                    tempValues.Add(Convert.ToDouble(ds.Tables[0].Rows[i]["paymentAmount"].ToString()));
                }
                catch
                {

                }
            }

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.category_chart.Series[0].Values = tempValues;
                this.category_chart.AxisY[0].Labels = LabelsList.ToArray();
            }));
        }

        public void RefreshData()
        {
            BackgroundWorker menuBackgroundWorker = new BackgroundWorker();
            BackgroundWorker todayBackgroundWorker = new BackgroundWorker();
            BackgroundWorker categoryBackgroundWorker = new BackgroundWorker();

            menuBackgroundWorker.DoWork += MenuBackgroundWorker_DoWork;
            todayBackgroundWorker.DoWork += TodayBackgroundWorker_DoWork;
            categoryBackgroundWorker.DoWork += CategoryBackgroundWorker_DoWork;

            menuBackgroundWorker.RunWorkerCompleted += MenuBackgroundWorker_RunWorkerCompleted;
            todayBackgroundWorker.RunWorkerCompleted += TodayBackgroundWorker_RunWorkerCompleted;
            categoryBackgroundWorker.RunWorkerCompleted += CategoryBackgroundWorker_RunWorkerCompleted;

            menuBackgroundWorker.RunWorkerAsync();
            todayBackgroundWorker.RunWorkerAsync();
            categoryBackgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region Background Workers

        private void CategoryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var categoryDataset = GetCategoryStatics();
            InitCategoryChart(categoryDataset);
        }

        private void TodayBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var todayDataset = GetTodayStatics();
            InitTodayChart(todayDataset);
        }

        private void MenuBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var menuDataset = GetMenuStatics();
            InitMenuChart(menuDataset);
        }

        private void CategoryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (todayBackgroundWorkerFinished == true && menuBackgroundWorkerFinished == true)
            {
                OnLoadFinished();
            }

            categoryBackgroundWorkerFinished = true;
        }

        private void TodayBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (categoryBackgroundWorkerFinished == true && menuBackgroundWorkerFinished == true)
            {
                OnLoadFinished();
            }

            todayBackgroundWorkerFinished = true;
        }

        private void MenuBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (categoryBackgroundWorkerFinished == true && todayBackgroundWorkerFinished == true)
            {
                OnLoadFinished();
            }

            menuBackgroundWorkerFinished = true;
        }

        #endregion

        #region Events

        private void OnLoadFinished()
        {
            categoryBackgroundWorkerFinished = false;
            todayBackgroundWorkerFinished = false;
            menuBackgroundWorkerFinished = false;

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.Loading.Visibility = Visibility.Hidden;
            }));
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.Loading.Visibility = Visibility.Visible;
            RefreshData();
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            this.Loading.Visibility = Visibility.Visible;

            RefreshData();
        }

        #endregion

        private void SendTodayAmount_Click(object sender, RoutedEventArgs e)
        {
            if(connectedToServer == false)
            {
                MessageBox.Show("서버와 연결이 끊긴 상태입니다. 다시 연결되면 자동으로 전송해드리겠습니다.");
            }

            App.socketController.sendMessage("@All" + "#스타벅스 오늘 판매량:" + todayAmount.ToString() + "원");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
            notify?.Dispose();
        }
    }
}
