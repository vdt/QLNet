/*
 Copyright (C) 2008 Siarhei Novik (snovik@gmail.com)
 Copyright (C) 2008 Toyin Akin (toyin_akin@hotmail.com)
 * 
 This file is part of QLNet Project http://www.qlnet.org

 QLNet is free software: you can redistribute it and/or modify it
 under the terms of the QLNet license.  You should have received a
 copy of the license along with this program; if not, license is  
 available online at <http://trac2.assembla.com/QLNet/wiki/License>.
  
 QLNet is a based on QuantLib, a free-software/open-source library
 for financial quantitative analysts and developers - http://quantlib.org/
 The QuantLib license is available online at http://quantlib.org/license.shtml.
 
 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace QLNet {
	// we need only one instance of the class
	// we can not derive it from IObservable because the class is static
	public static class Settings {

		[ThreadStatic]
		private static Date evaluationDate_ = null;

		[ThreadStatic]
		private static bool enforcesTodaysHistoricFixings_ = false;

		public static Date evaluationDate() 
		{
			if (evaluationDate_ == null)
				evaluationDate_ = Date.Today;
			return evaluationDate_; 
		}

		public static void setEvaluationDate(Date d) {
			evaluationDate_ = d;
			notifyObservers();
		}

		public static bool enforcesTodaysHistoricFixings {
			get { return enforcesTodaysHistoricFixings_; }
			set { enforcesTodaysHistoricFixings_ = value; }
		}

		////////////////////////////////////////////////////
		// Observable interface
		private static event Action notifyObserversEvent;

		public static void registerWith(Action handler) { notifyObserversEvent += handler; }
		public static void unregisterWith(Action handler) { notifyObserversEvent -= handler; }
		private static void notifyObservers() {
			Action handler = notifyObserversEvent;
			if (handler != null) {
				handler();
			}
		}
	}

	// helper class to temporarily and safely change the settings
	public class SavedSettings {
		private Date evaluationDate_;
		private bool enforcesTodaysHistoricFixings_;

		public SavedSettings() {
			evaluationDate_ = Settings.evaluationDate();
			enforcesTodaysHistoricFixings_ = Settings.enforcesTodaysHistoricFixings;
		}

		~SavedSettings() {
			if (evaluationDate_ != Settings.evaluationDate())
				Settings.setEvaluationDate(evaluationDate_);
			Settings.enforcesTodaysHistoricFixings = enforcesTodaysHistoricFixings_;
		}
	}
}