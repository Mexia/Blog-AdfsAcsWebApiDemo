// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValuesController.cs" company="">
//   
// </copyright>
// <summary>
//   The values controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Service.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    /// <summary>
    /// The values controller.
    /// </summary>
    public class ValuesController : ApiController
    {
        // GET api/values
        #region Public Methods and Operators

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public void Delete(int id)
        {
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.String].
        /// </returns>
        public IEnumerable<string> Get()
        {
            // This is just returning dummy data, you'll want to retrieve values from a Database
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public string Get(int id)
        {
            // This is just returning dummy data, you'll want to retrieve values from a Database
            return "value";
        }

        // POST api/values
        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        /// <summary>
        /// The put.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void Put(int id, [FromBody] string value)
        {
        }

        #endregion

        // DELETE api/values/5
    }
}