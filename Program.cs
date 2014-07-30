using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesForceDemo.SFDC;
using System.IO;
using System.Web.Services.Protocols;


namespace SalesForceDemo
{
    class Program
    {
        public static void Main(string[] args)
        {
            string userName = "dbryant@bpsdigitalmedia.com";
            string password = "integra123";
            string token = "7QxeTqpuHtOaiQ5Y9bFGWd1y8";


            SforceService binding = new SforceService();
            LoginResult CurrentLoginResult = null;

            try
            {
                CurrentLoginResult = binding.login(userName, String.Format("{0}{1}", password, token));
                
                string authEndPoint = binding.Url;
                binding.Url = CurrentLoginResult.serverUrl;

                binding.SessionHeaderValue = new SessionHeader();
                binding.SessionHeaderValue.sessionId = CurrentLoginResult.sessionId;

                // Make the describeGlobal() call
                DescribeGlobalResult dgr = binding.describeGlobal();


                // Get the sObjects from the describe global result
                DescribeGlobalSObjectResult[] sObjResults = dgr.sobjects;

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\temp\salesforce-objects.txt", true))
                {
                    // Write the name of each sObject to the console
                    for (int i = 0; i < sObjResults.Length; i++)
                    {
                        file.WriteLine(sObjResults[i].name);
                    }
                }


                describeSObjectsSample(binding);


               //string query = "SELECT Division__c FROM Lead";

               // var result = binding.queryAll(query);
               
            }

            catch (System.Web.Services.Protocols.SoapException e)
            {

                // This is likley to be caused by bad username or password

                binding = null;

                throw (e);

            }

            catch (Exception e)
            {

                // This is something else, probably comminication

                binding = null;

                throw (e);

            }

        }

        public static void describeSObjectsSample(SforceService binding)
        {
            try
            {
                // Call describeSObjectResults and pass it an array with
                // the names of the objects to describe.
                DescribeSObjectResult[] describeSObjectResults =
                                    binding.describeSObjects(
                                    new string[] {"Product_Family__c", "account", "contact"});

                // Iterate through the list of describe sObject results
                foreach (DescribeSObjectResult describeSObjectResult in describeSObjectResults)
                {

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\temp\sobject-results.txt", true))
                    {
                     
                    
                    // Get the name of the sObject
                    String objectName = describeSObjectResult.name;
                    file.WriteLine("sObject name: " + objectName);

                    // For each described sObject, get the fields
                    Field[] fields = describeSObjectResult.fields;

                    // Get some other properties
                    if (describeSObjectResult.activateable) file.WriteLine("\tActivateable");

                    // Iterate through the fields to get properties for each field
                    foreach (Field field in fields)
                    {
                        file.WriteLine("\tField: " + field.name);
                        file.WriteLine("\t\tLabel: " + field.label);
                        if (field.custom)
                            file.WriteLine("\t\tThis is a custom field.");
                        file.WriteLine("\t\tType: " + field.type);
                        if (field.length > 0)
                            file.WriteLine("\t\tLength: " + field.length);
                        if (field.precision > 0)
                            file.WriteLine("\t\tPrecision: " + field.precision);

                        // Determine whether this is a picklist field
                        if (field.type == fieldType.picklist)
                        {
                            // Determine whether there are picklist values
                            PicklistEntry[] picklistValues = field.picklistValues;
                            if (picklistValues != null && picklistValues[0] != null)
                            {
                                file.WriteLine("\t\tPicklist values = ");
                                for (int j = 0; j < picklistValues.Length; j++)
                                {
                                    file.WriteLine("\t\t\tItem: " + picklistValues[j].label);
                                }
                            }
                        }

                        // Determine whether this is a reference field
                        if (field.type == fieldType.reference)
                        {
                            // Determine whether this field refers to another object
                            string[] referenceTos = field.referenceTo;
                            if (referenceTos != null && referenceTos[0] != null)
                            {
                                file.WriteLine("\t\tField references the following objects:");
                                for (int j = 0; j < referenceTos.Length; j++)
                                {
                                    file.WriteLine("\t\t\t" + referenceTos[j]);
                                }
                            }
                        }
                    }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error has occurred: " + e.Message
                    + "\n" + e.StackTrace);
            }
        }
    }
}
