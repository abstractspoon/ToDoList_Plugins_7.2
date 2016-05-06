﻿
using System;
using TDLPluginHelpers;

// PLS DON'T ADD 'USING' STATEMENTS WHILE I AM STILL LEARNING!

namespace DayViewUIExtension
{
	public class CalendarItem : Calendar.Appointment
	{
		public DateTime OrgStartDate { get; set; }
		public DateTime OrgEndDate { get; set; }

		public String AllocTo { get; set; }
		public System.Boolean IsParent { get; set; }
	}

	public class DayViewUIExtensionCore : System.Windows.Forms.Panel, ITDLUIExtension
	{
		private IntPtr m_hwndParent;

		public DayViewUIExtensionCore(IntPtr hwndParent)
		{
			m_hwndParent = hwndParent;
			InitializeComponent();
		}

		private void CreateDayView()
		{
			Calendar.DrawTool drawTool = new Calendar.DrawTool();
			this.m_dayView = new Calendar.DayView();

			drawTool.DayView = this.m_dayView;
			this.m_dayView.ActiveTool = drawTool;
			this.m_dayView.AllowInplaceEditing = true;
			this.m_dayView.AmPmDisplay = false;
			this.m_dayView.AppHeightMode = Calendar.DayView.AppHeightDrawMode.TrueHeightAll;
			this.m_dayView.DaysToShow = 7;
			this.m_dayView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_dayView.DrawAllAppBorder = false;
			this.m_dayView.Font = new System.Drawing.Font("Tahoma", 8F);
			this.m_dayView.Location = new System.Drawing.Point(0, 0);
			this.m_dayView.MinHalfHourApp = false;
			this.m_dayView.Name = "m_dayView";
			this.m_dayView.Renderer = new Calendar.Office12Renderer();
			this.m_dayView.SelectionEnd = new System.DateTime(((long)(0)));
			this.m_dayView.SelectionStart = new System.DateTime(((long)(0)));
			this.m_dayView.Size = new System.Drawing.Size(798, 328);
			this.m_dayView.StartDate = DateTime.Now;
			this.m_dayView.TabIndex = 0;
			this.m_dayView.Text = "m_dayView";
			this.m_dayView.WorkingHourEnd = 19;
			this.m_dayView.WorkingHourStart = 9;
			this.m_dayView.WorkingMinuteEnd = 0;
			this.m_dayView.WorkingMinuteStart = 0;

            // I want the hour height to always be 20
            int hourHeight = 20;
            this.m_dayView.SlotsPerHour = 4;
            this.m_dayView.SlotHeight = (hourHeight / this.m_dayView.SlotsPerHour);

			this.m_dayView.StartDate = DateTime.Now;
			this.m_dayView.NewAppointment += new Calendar.NewAppointmentEventHandler(this.OnDayViewNewAppointment);
			this.m_dayView.SelectionChanged += new Calendar.AppointmentEventHandler(this.OnDayViewSelectionChanged);
			this.m_dayView.ResolveAppointments += new Calendar.ResolveAppointmentsEventHandler(this.OnDayViewResolveAppointments);
			this.m_dayView.AppointmentMove += new Calendar.AppointmentEventHandler(this.OnDayViewAppointmentChanged);

			this.m_dayView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnDayViewMouseMove);

			this.Controls.Add(m_dayView);

		}

		// ITDLUIExtension ------------------------------------------------------------------

		public bool SelectTask(UInt32 dwTaskID)
		{
			return true;
		}

		public bool SelectTasks(UInt32[] pdwTaskIDs)
		{
			return false;
		}

		public void UpdateTasks(TDLTaskList tasks, 
								TDLUIExtension.UpdateType type, 
								TDLUIExtension.TaskAttribute attrib)
		{
			TDLTask task = tasks.GetFirstTask();

			while (task.IsValid() && ProcessTaskUpdate(task, type, attrib))
				task = task.GetNextTask();

			Invalidate();
		}

		private bool ProcessTaskUpdate(TDLTask task, 
									   TDLUIExtension.UpdateType type,
									   TDLUIExtension.TaskAttribute attrib)
		{
			if (!task.IsValid())
				return false;

			CalendarItem item;

			if (m_Items.TryGetValue(task.GetID(), out item))
			{
				switch (attrib)
				{
					case TDLUIExtension.TaskAttribute.Title:
						item.Title = task.GetTitle();
						break;

					case TDLUIExtension.TaskAttribute.DoneDate:
						item.EndDate = item.OrgEndDate = ToDateTime(task.GetDoneDate());
						break;

					case TDLUIExtension.TaskAttribute.DueDate:
						item.EndDate = item.OrgEndDate = ToDateTime(task.GetDueDate());
						break;

					case TDLUIExtension.TaskAttribute.StartDate:
						item.StartDate = item.OrgStartDate = ToDateTime(task.GetStartDate());
						break;

					case TDLUIExtension.TaskAttribute.AllocTo:
						item.AllocTo = task.GetAllocatedTo(0);
						break;
				}
			}
			else
			{
				item = new CalendarItem();

				item.Title = task.GetTitle();
				item.EndDate = item.OrgEndDate = ToDateTime(task.GetDueDate());
				item.StartDate = item.OrgStartDate = ToDateTime(task.GetStartDate());
				item.AllocTo = task.GetAllocatedTo(0);
				item.Id = task.GetID();
				item.IsParent = task.IsParent();
			}

			if (item.EndDate > item.StartDate)
				m_Items[task.GetID()] = item;

			// Process children
			TDLTask subtask = task.GetFirstSubtask();

			while (subtask.IsValid() && ProcessTaskUpdate(subtask, type, attrib))
				subtask = subtask.GetNextTask();

			return true;
		}

		private System.DateTime ToDateTime(Int64 tDate)
		{
			return new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(tDate).ToLocalTime();
		}

		public bool WantEditUpdate(TDLUIExtension.TaskAttribute attrib)
		{
			switch (attrib)
			{
				case TDLUIExtension.TaskAttribute.Title:
				case TDLUIExtension.TaskAttribute.DoneDate:
				case TDLUIExtension.TaskAttribute.DueDate:
				case TDLUIExtension.TaskAttribute.StartDate:
				case TDLUIExtension.TaskAttribute.AllocTo:
					return true;
			}

			// all else
			return false;
		}
	   
		public bool WantSortUpdate(TDLUIExtension.TaskAttribute attrib)
		{
			return false;
		}
	   
		public bool PrepareNewTask(TDLTaskList task)
		{
			return false;
		}

		public bool ProcessMessage(IntPtr hwnd, UInt32 message, UInt32 wParam, UInt32 lParam, UInt32 time, Int32 xPos, Int32 yPos)
		{
			return false;
		}
	   
		public bool DoAppCommand(TDLUIExtension.AppCommand cmd, UInt32 dwExtra)
		{
			return false;
		}

		public bool CanDoAppCommand(TDLUIExtension.AppCommand cmd, UInt32 dwExtra)
		{
			return false;
		}

		public bool GetLabelEditRect(ref Int32 left, ref Int32 top, ref Int32 right, ref Int32 bottom)
		{
			return false;
		}

		public TDLUIExtension.HitResult HitTest(Int32 xPos, Int32 yPos)
		{
			return TDLUIExtension.HitResult.Nowhere;
		}

		public void SetUITheme(TDLTheme theme)
		{
			System.Windows.Media.Color temp = theme.GetAppColor(TDLTheme.AppColor.AppBackDark);

			this.BackColor = System.Drawing.Color.FromArgb(temp.R, temp.G, temp.B);
		}

		public void SetReadOnly(bool bReadOnly)
		{
		}

		public void SavePreferences(TDLPreferences prefs, String key)
		{
		}

		public void LoadPreferences(TDLPreferences prefs, String key, bool appOnly)
		{
		}
		
		// PRIVATE ------------------------------------------------------------------------------

		private void InitializeComponent()
		{
			this.BackColor = System.Drawing.Color.White;
			this.m_Items = new System.Collections.Generic.Dictionary<UInt32, CalendarItem>();

			CreateDayView();
		}

		private void OnDayViewNewAppointment(object sender, Calendar.NewAppointmentEventArgs args)
		{
//             Calendar.Appointment m_Appointment = new Calendar.Appointment();
// 
//             m_Appointment.StartDate = args.StartDate;
//             m_Appointment.EndDate = args.EndDate;
//             m_Appointment.Title = args.Title;
//             m_Appointment.Group = "2";
// 
//             m_Appointments.Add(m_Appointment);
		}

		private void OnDayViewMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
//             label2.Text = dayView1.GetTimeAt(e.X, e.Y).ToString();
		}

		private void OnDayViewSelectionChanged(object sender, Calendar.AppointmentEventArgs args)
		{
			TDLNotify notify = new TDLNotify(m_hwndParent);

			switch (this.m_dayView.Selection)
			{
				case Calendar.SelectionType.DateRange:
					break;

				case Calendar.SelectionType.Appointment:
					if (args.Appointment != null)
						notify.NotifySelChange(args.Appointment.Id);
					break;
			}
		}

		private void OnDayViewAppointmentChanged(object sender, Calendar.AppointmentEventArgs args)
		{
			if (args is Calendar.MoveAppointmentEventArgs)
			{
				TDLNotify notify = new TDLNotify(m_hwndParent);
				CalendarItem item = args.Appointment as CalendarItem;

				switch ((args as Calendar.MoveAppointmentEventArgs).Mode)
				{
					case Calendar.SelectionTool.Mode.Move:
						if ((item.StartDate - item.OrgStartDate).TotalSeconds != 0.0)
							notify.NotifyMod(TDLUIExtension.TaskAttribute.MoveTask, args.Appointment.StartDate);
						break;

					case Calendar.SelectionTool.Mode.ResizeTop:
						if ((item.StartDate - item.OrgStartDate).TotalSeconds != 0.0)
							notify.NotifyMod(TDLUIExtension.TaskAttribute.StartDate, args.Appointment.StartDate);
						break;

					case Calendar.SelectionTool.Mode.ResizeBottom:
						if ((item.EndDate - item.OrgEndDate).TotalSeconds != 0.0)
							notify.NotifyMod(TDLUIExtension.TaskAttribute.DueDate, args.Appointment.EndDate);
						break;
				}
			}
		}

		private void OnDayViewResolveAppointments(object sender, Calendar.ResolveAppointmentsEventArgs args)
		{
			System.Collections.Generic.List<Calendar.Appointment> appts = 
				new System.Collections.Generic.List<Calendar.Appointment>();

			foreach (System.Collections.Generic.KeyValuePair<UInt32, CalendarItem> item in m_Items)
			{
				if ((item.Value.StartDate >= args.StartDate) && (item.Value.EndDate <= args.EndDate))
				{
					appts.Add(item.Value);
				}
			}

			args.Appointments = appts;
		}

		// --------------------------------------------------------------------------------------
		private System.Collections.Generic.Dictionary<UInt32, CalendarItem> m_Items;
		private Calendar.DayView m_dayView;
	}
}
