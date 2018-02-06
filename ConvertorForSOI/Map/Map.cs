using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;

namespace ConvertorForSOI
{
    /// <summary>
    /// Класс Map предназначен, для соответствия данных из таблицы источника и результирующей таблицы. 
    /// </summary>
    public class Map
    {
        public Map()
        {
            listForDocForm2 = GetListForDocForm2();
            listForDocForm3 = GetListForDocForm3();
            listForDocForm4 = GetListForDocForm4();
            listForDocCards = GetListForDocCards();
            listForDocFormHz = GetListForDocFormHz();
            listForXls = GetListForXls();

            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".xls", MapConvertList = listForXls, TypeDoc = MapSourceItem.TypeDocument.Xls });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".xlsx", MapConvertList = listForXls, TypeDoc = MapSourceItem.TypeDocument.Xls });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".doc", MapConvertList = listForDocForm2, TypeDoc = MapSourceItem.TypeDocument.Form2 });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".doc", MapConvertList = listForDocFormHz, TypeDoc = MapSourceItem.TypeDocument.FormHz });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".doc", MapConvertList = listForDocForm3, TypeDoc = MapSourceItem.TypeDocument.Form3 });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 3, Extention = ".doc", MapConvertList = listForDocForm4, TypeDoc = MapSourceItem.TypeDocument.Form4 });
            mapSourceList.Add(new MapSourceItem() { RowNumber = 1, Extention = ".doc", MapConvertList = listForDocCards, TypeDoc = MapSourceItem.TypeDocument.DocCards });
        }

        public int GetRowNumberByTypeDocument(MapSourceItem.TypeDocument typeDocument)
        {
            int rowNumber = mapSourceList.Where(g => g.TypeDoc == typeDocument).Select(g => g.RowNumber).First();
            return rowNumber;
        }

        // Наименования колонок для результирующей таблицы.
        private Dictionary<string, string> dicDataColumns = new Dictionary<string, string>()
        {
            {"num", "VARCHAR"},
            {"code", "VARCHAR"},
            {"searchType", "VARCHAR"},
            {"source", "VARCHAR"},
            {"searchType_en", "VARCHAR"},
            {"photoList", "VARCHAR"},
            {"mark", "VARCHAR"},
            {"lastName", "VARCHAR"},
            {"firstName", "VARCHAR"},
            {"secondName", "VARCHAR"},
            {"lastName_en", "VARCHAR"},
            {"firstName_en", "VARCHAR"},
            {"secondName1_en", "VARCHAR"},
            {"daybirthDate", "VARCHAR"},
            {"monthbirthDate", "VARCHAR"},
            {"yearbirthDate", "VARCHAR"},
            {"mark_en", "VARCHAR"},
            {"birthPlace_en", "VARCHAR"},
            {"documentType", "VARCHAR"},
            {"data", "VARCHAR"},
            {"citizenship", "VARCHAR"},
            {"nazionalnost", "VARCHAR"},
            {"accusation", "VARCHAR"},
            {"accusation_en", "VARCHAR"},
            {"wantedInitiator_en", "VARCHAR"},
            {"searchNote", "VARCHAR"},
            {"wantedInitiator", "VARCHAR"},
            {"endDate", "VARCHAR"},
            {"beginDate", "VARCHAR"},
            {"sex", "VARCHAR"},
            {"birthPlace", "VARCHAR"}
        };

        private List<MapSourceItem> mapSourceList = new List<MapSourceItem>();

        public Dictionary<string, string> DataColumns
        {
            get { return dicDataColumns; }
        }

        public List<MapSourceItem> MapSource
        {
            get { return mapSourceList; }
        }

        // Карта соответствий для источника данных из .doc файла c Карточками.
        private List<MapConvertItem> listForDocCards;

        // Карта соответствий для источника данных из .doc файла созданного по Форме 2.
        private List<MapConvertItem> listForDocForm2;

        // Карта соответствий для источника данных из .doc файла созданного по Форме 3.
        private List<MapConvertItem> listForDocForm3;

        // Карта соответствий для источника данных из .doc файла созданного по Форме 4.
        private List<MapConvertItem> listForDocForm4;

        // Карта соответствий для источника данных из .doc файла созданного по непонятной форме.
        private List<MapConvertItem> listForDocFormHz;

        // Карта соответствий для источника данных из .xls файла.
        private List<MapConvertItem> listForXls;

        // Заполнение карты соответствия для источника данных из .doc файла созданного по хз что за форма.
        private List<MapConvertItem> GetListForDocFormHz()
        {
            List<MapConvertItem> ListForDocFormHz = new List<MapConvertItem>();
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //A. [CODE_OLD]          
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.Code, NumberCategory = -1 });            //B. [Code]              
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 0 });          //C. [lastName]          
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 3, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 1 });          //D. [firstName]         
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 4, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 2 });          //E. [secondName]        
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 5, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 0 });         //F. [daybirthDate]      
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 6, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 1 });         //G. [monthbirthDate]    
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 7, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 2 });         //H. [yearbirthDate]     
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 8, SourceTableIndex = 14, Category = ConvertCategory.FullName, NumberCategory = 0 });       //I. [lastName_en]       
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 9, SourceTableIndex = 14, Category = ConvertCategory.FullName, NumberCategory = 1 });       //J. [firstName_en]      
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 10, SourceTableIndex = 14, Category = ConvertCategory.FullName, NumberCategory = 2 });     //K. [secondName1_en]    
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 11, SourceTableIndex = 2, Category = ConvertCategory.Sex, NumberCategory = -1 });             //L. [Sex]               
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 12, SourceTableIndex = 5, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //M. [birthPlace]        
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 13, SourceTableIndex = 15, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //N. [birthPlace_en]     
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 14, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });      //O. [citizenship]       
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 15, SourceTableIndex = 6, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //P. [Mark]              
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 16, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Q. [mark_en]           
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 17, SourceTableIndex = 11, Category = ConvertCategory.WantedInitiator, NumberCategory = -1 });     //R. [wantedInitiator]   
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 18, SourceTableIndex = 8, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //S. [Accusation]        
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 19, SourceTableIndex = 17, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //T. [accusation_en]     
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 20, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //U. [Source]            
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 21, SourceTableIndex = 10, Category = ConvertCategory.SearchType, NumberCategory = -1 });     //V. [searchType]        
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 22, SourceTableIndex = 12, Category = ConvertCategory.BirthDate, NumberCategory = -1 });     //W. [beginDate]         
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 23, SourceTableIndex = 13, Category = ConvertCategory.BirthDate, NumberCategory = -1 });     //X. [endDate]           
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 24, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Y. [searchNote]        
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 25, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentType, NumberCategory = -1 }); //Z. [documentType]      
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 26, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentData, NumberCategory = -1 });     //AA.[Data]              
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 27, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AB.[wantedInitiator_en]
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 28, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AC.[naz]               
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 29, SourceTableIndex = -2, Category = ConvertCategory.Foto, NumberCategory = -1 });           //AD.[photoList]         
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 30, SourceTableIndex = -2, Category = ConvertCategory.Num, NumberCategory = -1 });            //AE.[num]             
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = 3, Category = ConvertCategory.PersonNote, NumberCategory = -2 });            //AE.[NICKNAME]  ADDED
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = 9, Category = ConvertCategory.PersonNote, NumberCategory = -2 });            //AE.[NICKNAME]  ADDED
            ListForDocFormHz.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 18, Category = ConvertCategory.PersonNote, NumberCategory = -2 });            //AE.[NICKNAME]  ADDED

            return ListForDocFormHz;
        }

        // Заполнение карты соответствия для источника данных из .doc файла созданного по Форме 2.
        private List<MapConvertItem> GetListForDocForm2()
        {
            List<MapConvertItem> ListForDocForm2 = new List<MapConvertItem>();
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //A. [CODE_OLD]          
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.Code, NumberCategory = -1 });            //B. [Code]              
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 0 });          //C. [lastName]          
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 3, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 1 });          //D. [firstName]         
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 4, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 2 });          //E. [secondName]        
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 5, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 0 });         //F. [daybirthDate]      
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 6, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 1 });         //G. [monthbirthDate]    
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 7, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 2 });         //H. [yearbirthDate]     
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 8, SourceTableIndex = 13, Category = ConvertCategory.FullName, NumberCategory = 0 });       //I. [lastName_en]       
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 9, SourceTableIndex = 13, Category = ConvertCategory.FullName, NumberCategory = 1 });       //J. [firstName_en]      
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 10, SourceTableIndex = 13, Category = ConvertCategory.FullName, NumberCategory = 2 });     //K. [secondName1_en]    
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 11, SourceTableIndex = 2, Category = ConvertCategory.Sex, NumberCategory = -1 });             //L. [Sex]               
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 12, SourceTableIndex = 5, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //M. [birthPlace]        
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 13, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 0 });      //N. [birthPlace_en]     
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 14, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });      //O. [citizenship]       
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 15, SourceTableIndex = 6, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //P. [Mark]              
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 16, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Q. [mark_en]           
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 17, SourceTableIndex = 11, Category = ConvertCategory.WantedInitiator, NumberCategory = -1 });     //R. [wantedInitiator]   
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 18, SourceTableIndex = 8, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //S. [Accusation]        
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 19, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //T. [accusation_en]     
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 20, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //U. [Source]            
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 21, SourceTableIndex = 10, Category = ConvertCategory.SearchType, NumberCategory = -1 });     //V. [searchType]        
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 22, SourceTableIndex = 12, Category = ConvertCategory.BirthDate, NumberCategory = -1 });     //W. [beginDate]         
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 23, SourceTableIndex = -3, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //X. [endDate]           
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 24, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Y. [searchNote]        
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 25, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentType, NumberCategory = -1 }); //Z. [documentType]      
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 26, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentData, NumberCategory = -1 });     //AA.[Data]              
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 27, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AB.[wantedInitiator_en]
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 28, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AC.[naz]               
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 29, SourceTableIndex = -2, Category = ConvertCategory.Foto, NumberCategory = -1 });           //AD.[photoList]         
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 30, SourceTableIndex = -2, Category = ConvertCategory.Num, NumberCategory = -1 });            //AE.[num]             
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = 3, Category = ConvertCategory.PersonNote, NumberCategory = -2 });            //AE.[NICKNAME]  ADDED
            ListForDocForm2.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = 9, Category = ConvertCategory.PersonNote, NumberCategory = -2 });            //AE.[NICKNAME]  ADDED

            return ListForDocForm2;
        }

        // Заполнение карты соответствия для источника данных из .doc файла созданного по Форме 3.
        private List<MapConvertItem> GetListForDocForm3()
        {
            List<MapConvertItem> ListForDocForm3 = new List<MapConvertItem>();
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //A. [CODE_OLD]          
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.Code, NumberCategory = -1 });            //B. [Code]              
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 0 });          //C. [lastName]          
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 3, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 1 });          //D. [firstName]         
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 4, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 2 });          //E. [secondName]        
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 5, SourceTableIndex = 3, Category = ConvertCategory.BirthDate, NumberCategory = 0 });         //F. [daybirthDate]      
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 6, SourceTableIndex = 3, Category = ConvertCategory.BirthDate, NumberCategory = 1 });         //G. [monthbirthDate]    
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 7, SourceTableIndex = 3, Category = ConvertCategory.BirthDate, NumberCategory = 2 });         //H. [yearbirthDate]     
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 8, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });       //I. [lastName_en]       
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 9, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //J. [firstName_en]      
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 10, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //K. [secondName1_en]    
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 11, SourceTableIndex = 2, Category = ConvertCategory.Sex, NumberCategory = -1 });             //L. [Sex]               
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 12, SourceTableIndex = 4, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //M. [birthPlace]        
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 13, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 0 });      //N. [birthPlace_en]     
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 14, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });      //O. [citizenship]       
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 15, SourceTableIndex = 7, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //P. [Mark]              
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 16, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Q. [mark_en]           
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 17, SourceTableIndex = -1, Category = ConvertCategory.WantedInitiator, NumberCategory = -1 });     //R. [wantedInitiator]   
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 18, SourceTableIndex = 6, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //S. [Accusation]        
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 19, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //T. [accusation_en]     
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 20, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //U. [Source]            
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 21, SourceTableIndex = -2, Category = ConvertCategory.SearchType, NumberCategory = -1 });     //V. [searchType]        
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 22, SourceTableIndex = -3, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //W. [beginDate]         
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 23, SourceTableIndex = 9, Category = ConvertCategory.StringDateString, NumberCategory = -1 });//X. [endDate]           
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 24, SourceTableIndex = 8, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //Y. [searchNote]        
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 25, SourceTableIndex = 5, Category = ConvertCategory.PersonDocumentType, NumberCategory = -1 });//Z. [documentType]      
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 26, SourceTableIndex = 5, Category = ConvertCategory.PersonDocumentData, NumberCategory = -1 });//AA.[Data]              
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 27, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AB.[wantedInitiator_en]
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 28, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AC.[naz]               
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 29, SourceTableIndex = -2, Category = ConvertCategory.Foto, NumberCategory = -1 });           //AD.[photoList]         
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 30, SourceTableIndex = -2, Category = ConvertCategory.Num, NumberCategory = -1 });            //AE.[num]               
            ListForDocForm3.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -1, Category = ConvertCategory.PersonNote, NumberCategory = -2 });

            return ListForDocForm3;
        }

        // TODO: Это пропавшие без вести, Результирующая таблица должна быть другая!
        // Заполнение карты соответствия для источника данных из .doc файла созданного по Форме 4.
        private List<MapConvertItem> GetListForDocForm4()
        {
            List<MapConvertItem> ListForDocForm4 = new List<MapConvertItem>();
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //A. [CODE_OLD]          
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.Code, NumberCategory = -1 });            //B. [Code]              
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 0 });          //C. [lastName]          
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 3, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 1 });          //D. [firstName]         
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 4, SourceTableIndex = 1, Category = ConvertCategory.FullName, NumberCategory = 2 });          //E. [secondName]        
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 5, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 0 });         //F. [daybirthDate]      
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 6, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 1 });         //G. [monthbirthDate]    
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 7, SourceTableIndex = 4, Category = ConvertCategory.BirthDate, NumberCategory = 2 });         //H. [yearbirthDate]     
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 8, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });       //I. [lastName_en]       
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 9, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //J. [firstName_en]      
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 10, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //K. [secondName1_en]    
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 11, SourceTableIndex = 2, Category = ConvertCategory.Sex, NumberCategory = -1 });             //L. [Sex]               
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 12, SourceTableIndex = 5, Category = ConvertCategory.NoCategory, NumberCategory = -1 });      //M. [birthPlace]        
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 13, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 0 });      //N. [birthPlace_en]     
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 14, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });      //O. [citizenship]       
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 15, SourceTableIndex = 6, Category = ConvertCategory.NoCategory, NumberCategory = 2 });       //P. [Mark]              
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 16, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Q. [mark_en]           
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 17, SourceTableIndex = 10, Category = ConvertCategory.WantedInitiator, NumberCategory = -1 });     //R. [wantedInitiator]   
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 18, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //S. [Accusation]        
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 19, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //T. [accusation_en]     
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 20, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //U. [Source]            
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 21, SourceTableIndex = -2, Category = ConvertCategory.SearchType, NumberCategory = -1 });     //V. [searchType]        
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 22, SourceTableIndex = 11, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //W. [beginDate]         
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 23, SourceTableIndex = 12, Category = ConvertCategory.StringDateString, NumberCategory = -1 });//X. [endDate]           
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 24, SourceTableIndex = 13, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //Y. [searchNote]        
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 25, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentType, NumberCategory = -1 });//Z. [documentType]      
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 26, SourceTableIndex = 7, Category = ConvertCategory.PersonDocumentData, NumberCategory = -1 });//AA.[Data]              
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 27, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AB.[wantedInitiator_en]
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 28, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });     //AC.[naz]               
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 29, SourceTableIndex = -2, Category = ConvertCategory.Foto, NumberCategory = -1 });           //AD.[photoList]         
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 30, SourceTableIndex = -2, Category = ConvertCategory.Num, NumberCategory = -1 });            //AE.[num]               
            ListForDocForm4.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = 3, Category = ConvertCategory.PersonNote, NumberCategory = -2 });

            return ListForDocForm4;
        }

        // Заполнение карты соответствия для источника данных из .doc файла созданного по Форме DocКарточки.
        private List<MapConvertItem> GetListForDocCards()
        {
            List<MapConvertItem> ListForDocCards = new List<MapConvertItem>();
            // Просто чтобы был ListForDocCards.Count > 0
            ListForDocCards.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = 0, Category = ConvertCategory.NoCategory, NumberCategory = 0 });

            return ListForDocCards;
        }

        // Заполнение карты соответствия для источника данных из .xls файла.
        private List<MapConvertItem> GetListForXls()
        {
            List<MapConvertItem> ListForXls = new List<MapConvertItem>();
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 0, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });   //A. [CODE_OLD]          
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.Code, NumberCategory = -1 });         //B. [Code]              
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 2, SourceTableIndex = 0, Category = ConvertCategory.FullName, NumberCategory = 0 });       //C. [lastName]          
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 3, SourceTableIndex = 0, Category = ConvertCategory.FullName, NumberCategory = 1 });       //D. [firstName]         
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 4, SourceTableIndex = 0, Category = ConvertCategory.FullName, NumberCategory = 2 });       //E. [secondName]        
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 5, SourceTableIndex = 1, Category = ConvertCategory.BirthDate, NumberCategory = 0 });      //F. [daybirthDate]      
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 6, SourceTableIndex = 1, Category = ConvertCategory.BirthDate, NumberCategory = 1 });      //G. [monthbirthDate]    
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 7, SourceTableIndex = 1, Category = ConvertCategory.BirthDate, NumberCategory = 2 });      //H. [yearbirthDate]     
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 8, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });    //I. [lastName_en]       
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 9, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 2 });    //J. [firstName_en]      
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 10, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //K. [secondName1_en]    
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 11, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //L. [Sex]               
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 12, SourceTableIndex = 2, Category = ConvertCategory.NoCategory, NumberCategory = -1 });   //M. [birthPlace]        
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 13, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 0 });   //N. [birthPlace_en]     
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 14, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = 1 });   //O. [citizenship]       
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 15, SourceTableIndex = 3, Category = ConvertCategory.NoCategory, NumberCategory = 2 });    //P. [Mark]              
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 16, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //Q. [mark_en]           
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 17, SourceTableIndex = 7, Category = ConvertCategory.WantedInitiator, NumberCategory = -1 });   //R. [wantedInitiator]   
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 18, SourceTableIndex = 5, Category = ConvertCategory.NoCategory, NumberCategory = -1 });   //S. [Accusation]        
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 19, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //T. [accusation_en]     
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 20, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //U. [Source]            
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 21, SourceTableIndex = 5, Category = ConvertCategory.SearchType, NumberCategory = -1 });   //V. [searchType]        
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 22, SourceTableIndex = -3, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //W. [beginDate]         
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 23, SourceTableIndex = -3, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //X. [endDate]           
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 24, SourceTableIndex = 9, Category = ConvertCategory.NoCategory, NumberCategory = -1 });   //Y. [searchNote]        
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 25, SourceTableIndex = 4, Category = ConvertCategory.PersonDocumentType, NumberCategory = -1 });//Z. [documentType]      
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 26, SourceTableIndex = 4, Category = ConvertCategory.PersonDocumentData, NumberCategory = -1 });//AA.[Data]              
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 27, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //AB.[wantedInitiator_en]
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 28, SourceTableIndex = -1, Category = ConvertCategory.NoCategory, NumberCategory = -1 });  //AC.[naz]               
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 29, SourceTableIndex = -2, Category = ConvertCategory.Foto, NumberCategory = -1 });        //AD.[photoList]         
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 30, SourceTableIndex = -2, Category = ConvertCategory.Num, NumberCategory = -1 });         //AE.[num]               
            ListForXls.Add(new MapConvertItem() { ResultTableIndex = 1, SourceTableIndex = -2, Category = ConvertCategory.PersonNote, NumberCategory = -2 });

            return ListForXls;
        }

        /// <summary>
        /// Метод конвертирует строковое значение даты, конвертирует его в DateTime, потом опять в строковое значение даты.
        /// Делается это с целью записи в базу данных корректного значения даты в виде строки.
        /// </summary>
        /// <param name="strDate">Дата в виде строки.</param>
        /// <returns>Дата в виде строки. Если форматировать не удалось, то пустая строка.</returns>
        public static string StringDateString(string strDate)
        {
            // Разбиваем на массив строк
            string[] strDates = strDate.Split(' ');

            // Для определения самой ранней даты
            DateTime minDate = DateTime.Now;
            // Для контроля, отличается ли в итоге дата от сегодняшней
            DateTime controlDate = minDate;

            // Вырежем все кроме цифр и точек в датах
            // и вычислим самую раннюю дату
            Regex rgx = new Regex("[^\\d.]");
            foreach (string date in strDates)
            {
                DateTime newDate;
                if (DateTime.TryParse(rgx.Replace(date, "").Trim('.'), out newDate))
                {
                    // Если текущая проверочная дата раньше чем minDate, присвоим ее minDate
                    if (newDate <= minDate)
                    {
                        minDate = newDate;
                    }
                }
            }
            return minDate < controlDate ? minDate.ToString("yyyy.MM.dd") : String.Empty;

        }

        /// <summary>
        /// Получаем значение следующего номера code, на основании предыдущего номера code. 
        /// Не важно откуда он пришел из базы или из приложения.
        /// </summary>
        /// <returns>/Сформированный следующий номер code в виде int.</returns>

        public static string GetNextCode()
        {
            //DateTime currentDate = DateTime.Now;
            //string timeStamp = currentDate.ToString("yyyyMMddHHmmssfff");
            ////int temp = int.Parse(timeStamp);
            //string rand = new Random(int.Parse(currentDate.ToString("mmssfff"))).Next().ToString(); // некий рандом
            //if(rand.Length < 5)
            //{
            //    rand = "0000" + rand;
            //}
            //rand = rand.Substring((rand.Length-4));
            string guid = Guid.NewGuid().ToString();
            return guid;
        }

        /// <summary>
        /// Получаем номер num, на основе сформированного номера code.
        /// </summary>
        /// <param name="code"> Сформированный номер  code.</param>
        /// <returns>Номер num.</returns>
        /// Рудемент
        //public static string GetNumByCode(string code)
        //{
        //    return code.Substring(6);
        //}

        /// <summary>
        /// Метод для заполнения поля Пол(Sex).
        /// </summary>
        /// <param name="sex">Строка с тем как это поле заполнено в исходнике.</param>
        /// <returns>Строка с тем, как должно быть заполнено в базе. Если нет соответствия, то возвращаем пустую строку, данное поле не будет заполнено.</returns>
        public static string GetSex(string sex)
        {
            if (sex.ToLower().Contains("муж"))
                return "Мужчина";
            else if (sex.ToLower().Contains("жен"))
                return "Женщина";
            else return "";
        }

        /// <summary>
        /// Метод получает список путей к файлам с фотографиями.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFotoFileList(string fotoPath, FullName fullName, BirthDate birthDate)
        {
            if(string.IsNullOrWhiteSpace(fullName.Names[0]))
            {
                return new List<string>();
            }

                if (!Directory.Exists(fotoPath))
            {
                MessageBox.Show("Выберете корректный путь к папке с фотографиями.");
                return null;
            }
            string[] fotoFiles = Directory.GetFiles(fotoPath);
            List<string> fotoFileNames = fotoFiles.Where(g=>(fullName != null ? (
                (!string.IsNullOrWhiteSpace(fullName.Names[0]) ? g.ToLower().Contains(fullName.Names[0].ToLower().Trim()) : true)&&
                (!string.IsNullOrWhiteSpace(fullName.Names[1]) ? g.ToLower().Contains(fullName.Names[1].ToLower().Trim()) : true) &&
                (!string.IsNullOrWhiteSpace(fullName.Names[2]) ? g.ToLower().Contains(fullName.Names[2].ToLower().Trim()) : true)
                ) : true)
                &&
                (birthDate != null ? (
                (!string.IsNullOrWhiteSpace(birthDate.BirthDates[0]) ? g.ToLower().Contains(birthDate.BirthDates[0].ToLower().Trim()) : true) &&
                (!string.IsNullOrWhiteSpace(birthDate.BirthDates[1]) ? g.ToLower().Contains(birthDate.BirthDates[3].ToLower().Trim()) : true) &&
                (!string.IsNullOrWhiteSpace(birthDate.BirthDates[2]) ? g.ToLower().Contains(birthDate.BirthDates[2].ToLower().Trim()) : true)) : true)
                ).ToList();

            return fotoFileNames;
        }
    }
}
