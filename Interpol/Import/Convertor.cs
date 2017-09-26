using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using GenericParsing;

namespace SZI.Import
{
    public class Convertor
    {
        protected string ConnectionString { get; set; }

        protected FileInfo ImportFile { get; set; }



        public Convertor(FileInfo fileInfo)
        {
            ImportFile = fileInfo;
        }



        /*
        private bool LogInsert()
        {
            try
            {
                SqlCommand insertCommand = new SqlCommand(
                    $@"
                    INSERT INTO [{_logTableName}]
                        ([Created]
                        ,[Event]
                        ,[UserName]
                        ,[IOType]
                        ,[Info]
                        ,[Misc])
                    VALUES
                        (convert(datetime2, '{_created.ToString()}', 104)
                        ,'{_eventType.ToString()}'
                        ,'{_user}'
                        ,'{_iOType}'
                        ,'{_info}'
                        ,'IP: {_userIP}<br>{_misc}')
                    ",
                    _connection);

                SqlDataAdapter dataAdapter =
                new SqlDataAdapter();

                _connection.Open();

                dataAdapter.InsertCommand = insertCommand;
                dataAdapter.InsertCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LoggerObject.LogException(ex);
                return false;
            }
            finally
            {
                _connection.Close();
            }
            return true;
        }
        */
    }
}