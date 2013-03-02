/*
This file is part of Keepass2Android, Copyright 2013 Philipp Crocoll. This file is based on Keepassdroid, Copyright Brian Pellin.

  Keepass2Android is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 2 of the License, or
  (at your option) any later version.

  Keepass2Android is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with Keepass2Android.  If not, see <http://www.gnu.org/licenses/>.
  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using KeePassLib;
using Android.Text;
using Android.Text.Style;

namespace keepass2android.view
{
	
	public class PwEntryView : ClickView {
		
		protected GroupBaseActivity mAct;
		protected PwEntry mPw;
		private TextView mTv;
		private int mPos;
		
		protected const int MENU_OPEN = Menu.First;
		private const int MENU_DELETE = MENU_OPEN + 1;
		
		public static PwEntryView getInstance(GroupBaseActivity act, PwEntry pw, int pos) 
		{
			return new PwEntryView(act, pw, pos);

		}

		public PwEntryView (IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			
		}
		
		protected PwEntryView(GroupBaseActivity act, PwEntry pw, int pos):base(act) {
			mAct = act;
			
			View ev = View.Inflate(mAct, Resource.Layout.entry_list_entry, null);
			mTv = (TextView) ev.FindViewById(Resource.Id.entry_text);
			mTv.TextSize = PrefsUtil.getListTextSize(act);
			
			populateView(ev, pw, pos);
			
			LayoutParams lp = new LayoutParams(LayoutParams.FillParent, LayoutParams.WrapContent);
			
			AddView(ev, lp);
			
		}
		
		private void populateView (View ev, PwEntry pw, int pos)
		{
			mPw = pw;
			mPos = pos;
			
			ImageView iv = (ImageView)ev.FindViewById (Resource.Id.entry_icon);
			bool isExpired = pw.Expires && pw.ExpiryTime < DateTime.Now;
			if (isExpired) {
				App.getDB ().drawFactory.assignDrawableTo (iv, Resources, App.getDB ().pm, PwIcon.Expired, PwUuid.Zero);
			}
			else
			{
				App.getDB ().drawFactory.assignDrawableTo (iv, Resources, App.getDB ().pm, pw.IconId, pw.CustomIconUuid);
			}

			String title = pw.Strings.ReadSafe(PwDefs.TitleField);
			var str = new SpannableString(title);

			if (isExpired)
			{
				str.SetSpan(new StrikethroughSpan(), 0, title.Length, SpanTypes.ExclusiveExclusive);
			}
			mTv.TextFormatted = str;


		}
		
		public void convertView(PwEntry pw, int pos) {
			populateView(this, pw, pos);
		}
		

		public override void OnClick() {
			launchEntry();
		}
		
		private void launchEntry() {
			mAct.LaunchActivityForEntry(mPw,mPos);
			
		}
		
		public override void OnCreateMenu(IContextMenu menu, IContextMenuContextMenuInfo menuInfo) {
			menu.Add(0, MENU_OPEN, 0, Resource.String.menu_open);
			menu.Add(0, MENU_DELETE, 0, Resource.String.menu_delete);
		}
		
		public override bool OnContextItemSelected(IMenuItem item) {
			switch ( item.ItemId) {
				
			case MENU_OPEN:
				launchEntry();
				return true;
			case MENU_DELETE:
				Handler handler = new Handler();
				DeleteEntry task = new DeleteEntry(Context, App.getDB(), mPw, new GroupBaseActivity.RefreshTask(handler, mAct));
				ProgressTask pt = new ProgressTask(mAct, task, Resource.String.saving_database);
				pt.run();
				return true;
			
			default:
				return false;
			}
		}

		
	}
}
