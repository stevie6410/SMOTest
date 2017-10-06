using System;
using System.Web.Http;

namespace SMO_Test.Controllers
{
    [RoutePrefix("api/smo")]
    public class SMOController : ApiController
    {
        [HttpGet]
        [Route("script")]
        public IHttpActionResult Get()
        {
            try
            {
                using (var scripter = new SqlObjectScripter("ServerName", "DatabaseName", "username", "password"))
                {
                    var result = scripter.GenerateScript("dbo", "testObject", "TABLE");
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
