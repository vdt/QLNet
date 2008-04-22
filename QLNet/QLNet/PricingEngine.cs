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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QLNet
{
    //! abstract class for pricing engines
    public abstract class PricingEngine : IObservable
    {
        public abstract PricingEngine.Arguments getArguments();
        public abstract PricingEngine.Results getResults();
        public abstract void reset();
        public virtual void calculate() { }

        public abstract class Arguments
        {
            public abstract void validate();
        }
        public abstract class Results
        {
            public abstract void reset();
        }

        // observable interface
        public event Callback notifyObserversEvent;
        public void registerWith(Callback handler) { notifyObserversEvent += handler; }
        public void unregisterWith(Callback handler) { notifyObserversEvent -= handler; }
        protected void notifyObservers()
        {
            Callback handler = notifyObserversEvent;
            if (handler != null)
            {
                handler();
            }
        }
    }

    // template base class for option pricing engines
    // Derived engines only need to implement the <tt>calculate()</tt> method.
    public abstract class GenericEngine<ArgumentsType, ResultsType> : PricingEngine, IObserver
        where ArgumentsType : PricingEngine.Arguments, new()
        where ResultsType : PricingEngine.Results, new()
    {
        protected ArgumentsType arguments_ = new ArgumentsType();
        protected ResultsType results_ = new ResultsType();

        public override PricingEngine.Arguments getArguments() { return arguments_; }
        public override PricingEngine.Results getResults() { return results_; }
        public override void reset() { results_.reset(); }

        public void update() { notifyObservers(); }
    };
}
