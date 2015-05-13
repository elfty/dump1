using System;
using System.Collections;

using YJ.AppLink.Api;

namespace YJ.AppLink.Pricing
{
	public interface IMarketFilter
	{
		bool PassesFilter (MarketData mkt);
	}

	public abstract class CompositeFilter : IMarketFilter
	{
		protected ArrayList mFilters = new ArrayList();
		
		protected CompositeFilter (IMarketFilter[] filters)
		{
			foreach (IMarketFilter filter in filters)
			{
				mFilters.Add(filter);
			}
		}

		protected CompositeFilter (IMarketFilter filter1, IMarketFilter filter2)
		{
			mFilters.Add(filter1);
			mFilters.Add(filter2);
		}

		private CompositeFilter () {}
		
		#region public methods

		public void AddFilter (IMarketFilter filter)
		{
			mFilters.Add(filter);
		}

		public void RemoveFilter (IMarketFilter filter)
		{
			if (mFilters.Contains(filter))
				mFilters.Remove(filter);
		}

		#endregion

		#region IMarketFilter Members

		public abstract bool PassesFilter(MarketData mkt);

		#endregion

	}

	public class AndMarketFilter : CompositeFilter
	{
		public AndMarketFilter (IMarketFilter filter1, IMarketFilter filter2):
			base (filter1, filter2) {}

		public AndMarketFilter (IMarketFilter[] filters):base(filters) {}

		#region IMarketFilter Members

		public override bool PassesFilter(MarketData mkt)
		{
			if (mFilters.Count == 0)
				return true;

			foreach (IMarketFilter filter in mFilters)
			{
				if (!filter.PassesFilter(mkt))
					return false;
			}

			return true;
		}

		#endregion
	}

	public class OrMarketFilter : CompositeFilter
	{
		public OrMarketFilter (IMarketFilter filter1, IMarketFilter filter2):
			base (filter1, filter2) {}

		public OrMarketFilter (IMarketFilter[] filters):base(filters) {}

		#region IMarketFilter Members

		public override bool PassesFilter(MarketData mkt)
		{
			if (mFilters.Count == 0)
				return true;

			foreach (IMarketFilter filter in mFilters)
			{
				if (filter.PassesFilter(mkt))
					return true;
			}

			return false;
		}

		#endregion
	}

	public class NotMarketFilter : IMarketFilter
	{
		protected IMarketFilter filter;
		
		public NotMarketFilter(IMarketFilter filter)
		{
			this.filter = filter;
		}
			
		#region IMarketFilter Members

		public bool PassesFilter(MarketData mkt)
		{
			if (filter == null)
				return true;

			return !filter.PassesFilter(mkt);
		}

		#endregion
	}

	public class HasTermFilter : IMarketFilter
	{
        private string term;
		
		public HasTermFilter(string term) 
		{
            this.term = term.ToUpper();
		}
		
		#region IMarketFilter Members

		public bool PassesFilter(MarketData mkt)
		{
			
			if (mkt.Options != null &&
                mkt.Options.Length > 0)
			{
                foreach (Option leg in mkt.Options)
				{
                    if (leg.Term.Equals(term))
                        return true;

				}
			}

			return false;

		}
	
		#endregion
		

	}
}

