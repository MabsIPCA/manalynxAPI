using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using System.IdentityModel.Tokens.Jwt;

namespace ManaLynxAPI.Utils
{

    public interface ILoggerUtils
    {
        void SetLogInfoGetAll(int? manaUserId, string controller);
        void SetLogInfoGet(int? manaUserId, string controller, int? id);
        void SetLogInfoPost(int? manaUserId, string controller, string data);
        void SetLogInfoPut(int? manaUserId, string controller, string data);
        void SetLogInfoDelete(int? manaUserId, string controller, int? id);
    }

    public class LoggerUtils : ILoggerUtils
    {
        private readonly ILogger<LoggerUtils> _logger;

        public LoggerUtils(ILogger<LoggerUtils> logger)
        {
            _logger = logger;
        } 

        /// <summary>
        /// Write GetAll request to log file
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <param name="controller">name of controller used</param>
        /// <returns>ClienteId</returns>
        public void SetLogInfoGetAll(int? manaUserId, string controller)
        {
            _logger.LogInformation("--> GetAll request by user " + manaUserId + " using controller " + controller);
        }

        /// <summary>
        /// Write Get request to log file
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <param name="controller">name of controller used</param>
        /// <param name="id">get request to this id</param>
        /// <returns>ClienteId</returns>
        public void SetLogInfoGet(int? manaUserId, string controller, int? id)
        {
            _logger.LogInformation("--> Get request by user " + manaUserId + " using controller " + controller + " to id " + id);
        }

        /// <summary>
        /// Write Post request to log file
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <param name="controller">name of controller used</param>
        /// <param name="data">data posted to DB</param>
        /// <returns>ClienteId</returns>
        public void SetLogInfoPost(int? manaUserId, string controller, string data)
        {
            _logger.LogInformation("--> Post request by user " + manaUserId + " using controller " + controller + " posted " + data);
        }

        /// <summary>
        /// Write Put request to log file
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <param name="controller">name of controller used</param>
        /// <param name="data">data posted to DB</param>
        /// <returns>ClienteId</returns>
        public void SetLogInfoPut(int? manaUserId, string controller, string data)
        {
            _logger.LogInformation("--> Put request by user " + manaUserId + " using controller " + controller + " posted " + data);
        }

        /// <summary>
        /// Write Post request to log file
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <param name="controller">name of controller used</param>
        /// <param name="id">id of row to delete</param>
        /// <returns>ClienteId</returns>
        public void SetLogInfoDelete(int? manaUserId, string controller, int? id)
        {
            _logger.LogInformation("--> Delete request by user " + manaUserId + " using controller " + controller + " deleted id " + id);
        }
    }
}
