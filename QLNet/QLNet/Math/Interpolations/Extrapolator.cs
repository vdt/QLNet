/*
 Copyright (C) 2008 Siarhei Novik (snovik@gmail.com)
  
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

using QLNet.Patterns;

namespace QLNet
{
    /// <summary>
	/// Base class for classes possibly allowing extrapolation.
    /// </summary>
    /// <remarks>
	/// <seealso cref="LazyObject"/> should not be here but it is because of the <seealso cref="InterpolatedYieldCurve"/>.
	/// </remarks>
    public abstract class Extrapolator : LazyObject
    {
        private bool extrapolate_;
    	public bool extrapolate
    	{
    		get { return extrapolate_; } 
			set { extrapolate_ = value; }
    	}

        /// <summary>
		/// Tells whether extrapolation is enabled
        /// </summary>
        /// <returns></returns>
        public bool allowsExtrapolation()
        {
        	return extrapolate_;
        }

		/// <summary>
		/// Enable extrapolation in subsequent calls
		/// </summary>
        public void enableExtrapolation()
        {
        	extrapolate_ = true;
        }

		/// <summary>
		/// Disable extrapolation in subsequent calls
		/// </summary>
        public void disableExtrapolation()
        {
        	extrapolate_ = false;
        }
    }
}
