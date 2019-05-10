using System;
using System.Collections.Generic;

namespace Banana
{
    /// <summary>
    /// Holds the results of a paged request.
    /// </summary>
    /// <typeparam name="T">The type of Poco in the returned result set</typeparam>
    public class PagingResult<T>
    {
        /// <summary>
        ///     The current page number contained in this page of result set
        /// </summary>
        public long Page { get; set; }

        /// <summary>
        ///     The total number of pages in the full result set
        /// </summary>
        public long PageCount { get; set; }

        /// <summary>
        ///     The total number of records in the full result set
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        ///     The number of items per page
        /// </summary>
        public long Limit { get; set; }

        ///// <summary>
        /////     The actual records on this page
        ///// </summary>
        //public List<T> Items { get; set; }

        /// <summary>
        ///     User property to hold anything.
        /// </summary>
        public object Context { get; set; }

        public List<T> Data { get; set; }

        public override string ToString()
        {
            return this.ToObject().ToJSON();
        }

        public object ToObject()
        {
            return new
            {
                context = this.Context,
                data = this.Data,
                page = this.Page,
                limit = this.Limit,
                pageCount = this.PageCount,
                count = this.Count
            };
        }
    }
}