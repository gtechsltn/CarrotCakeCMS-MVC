﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Carrotware.CMS.UI.Components {
	public class LiteralMessage : IHtmlString {

		public LiteralMessage() { }

		public LiteralMessage(Exception ex, string key, string path) {
			string msg = String.Empty;

			if (ex != null) {
				msg = String.Format("<pre>{0}</pre>", ex);

				if (ex.InnerException != null) {
					msg = String.Format("<pre>\r\n{0}\r\n{1}\r\n</pre>", ex, ex.InnerException);
				}
			} else {
				msg = "<p>There was an error loading the widget.</p>";
			}

			this.Message = string.Format("<div>\r\n<p><b>{0}:</b>  {1}</p>\r\n{2}</div>", key, path, msg);
		}

		public string Message { get; set; }


		public string ToHtmlString() {
			return this.Message;
		}
	}
}