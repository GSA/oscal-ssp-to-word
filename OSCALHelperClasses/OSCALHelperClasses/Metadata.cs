using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Data;

namespace OSCALHelperClasses
{
    public class Metadata : OSCALBaseClass
    {
        public Item Title;
        public DateItem Published;
        public DateItem LastModified;
        public Item Version;
        public Item OSCALVersion;
        public List<Revision> Revisions;
        public List<Property> Properties;
        public List<Role> Roles;
        public List<Party> Parties;
        public List<ResponsibleParty> ResponsibleParties;
        public List<Organization> Organizations;
        public List<Location> Locations;

        public Metadata()
        {

        }
        public Metadata(XmlNode node)
        {
            if (node.Name != "metadata")
                return;
            Properties = new List<Property>();
            Roles = new List<Role>();
            Parties = new List<Party>();
            ResponsibleParties = new List<ResponsibleParty>();
            Organizations = new List<Organization>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "title")
                {
                    Title = new Item();
                    Title.Value = child.InnerText;
                    Title.XPath = FindDeepestPath(child);
                }

                if (child.Name == "published")
                {
                    Published = new DateItem();

                    DateTime.TryParse(child.InnerText, out Published.Date);
                    Published.XPath = FindDeepestPath(child);

                }

                if (child.Name == "last-modified")
                {
                    LastModified = new DateItem();
                    DateTime.TryParse(child.InnerText, out LastModified.Date);
                    LastModified.XPath = FindDeepestPath(child);
                }

                if (child.Name == "version")
                {
                    Version = new Item();
                    Version.Value = child.InnerText;
                    Version.XPath = FindDeepestPath(child);
                }

                if (child.Name == "oscal-version")
                {
                    OSCALVersion = new Item();
                    OSCALVersion.Value = child.InnerText;
                    OSCALVersion.XPath = FindDeepestPath(child);
                }

                if (child.Name == "prop")
                {

                    var prop = new Property();
                    prop.Name = child.Attributes[0].Value;
                    prop.Value = child.InnerText;
                    prop.XPath = FindDeepestPath(child);

                    Properties.Add(prop);
                }

                if (child.Name == "role")
                {
                    var role = new Role();

                    role.RoleID = child.Attributes[0].Value;
                    role.Title = new Item();
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {
                        if (grandchild.Name == "title")
                        {
                            role.Title.Value = grandchild.InnerText;
                            role.Title.XPath = FindDeepestPath(grandchild);
                        }
                    }

                    Roles.Add(role);
                }

                if (child.Name == "party")
                {
                    var party = new Party();
                    party.PartyID = child.Attributes[0].Value;
                    foreach (XmlNode grandchild in child)
                    {
                        if (grandchild.Name == "org")
                        {
                            party.Organization = GetOrganization(grandchild);
                            Organizations.Add(party.Organization);
                        }
                        if (grandchild.Name == "person")
                        {
                            party.Person = GetPerson(grandchild);

                        }

                    }

                    Parties.Add(party);
                }
                if (child.Name == "responsible-party")
                {
                    var responsibleparty = new ResponsibleParty();
                    responsibleparty.RoleID = child.Attributes[0].Value;
                    responsibleparty.PartyIDs = new List<Item>();
                    foreach (XmlNode grandchild in child)
                    {
                        if (grandchild.Name == "party-id")
                        {
                            var partId = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            responsibleparty.PartyIDs.Add(partId);
                        }

                        ResponsibleParties.Add(responsibleparty);
                    }
                }

            }


        }
        public Metadata(XmlNode node, bool Milestone)
        {
            if (node.Name != "metadata")
                return;
            Properties = new List<Property>();
            Roles = new List<Role>();
            Parties = new List<Party>();
            ResponsibleParties = new List<ResponsibleParty>();
            Organizations = new List<Organization>();
            Revisions = new List<Revision>();
            Locations = new List<Location>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "title")
                {
                    Title = new Item();
                    Title.Value = child.InnerText;
                    Title.XPath = FindDeepestPath(child);
                }

                if (child.Name == "published")
                {
                    Published = new DateItem();

                    DateTime.TryParse(child.InnerText, out Published.Date);
                    Published.XPath = FindDeepestPath(child);

                }

                if (child.Name == "last-modified")
                {
                    LastModified = new DateItem();
                    DateTime.TryParse(child.InnerText, out LastModified.Date);
                    LastModified.XPath = FindDeepestPath(child);
                }

                if (child.Name == "version")
                {
                    Version = new Item();
                    Version.Value = child.InnerText;
                    Version.XPath = FindDeepestPath(child);
                }

                if (child.Name == "oscal-version")
                {
                    OSCALVersion = new Item();
                    OSCALVersion.Value = child.InnerText;
                    OSCALVersion.XPath = FindDeepestPath(child);
                }

                if (child.Name == "prop")
                {

                    var prop = new Property();
                    prop.Name = child.Attributes[0].Value;
                    prop.Value = child.InnerText;
                    prop.XPath = FindDeepestPath(child);
                    Properties.Add(prop);
                }

                if (child.Name == "role")
                {
                    var role = new Role();

                    role.RoleID = child.Attributes[0].Value;
                    role.Title = new Item();
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {
                        if (grandchild.Name == "title")
                        {
                            role.Title.Value = grandchild.InnerText;
                            role.Title.XPath = FindDeepestPath(grandchild);
                        }
                    }

                    Roles.Add(role);
                }
                if(child.Name == "location")
                {
                    var location = new Location();
                    location.UUID = child.Attributes[0].Value;
                    foreach(XmlNode grandchild in child)
                    {
                        if(grandchild.Name=="title")
                        {
                            location.Title.Value = grandchild.InnerText;
                            location.Title.XPath = FindDeepestPath(grandchild);
                        }
                        if (grandchild.Name == "address")
                        {
                            location.Address = GetAddress(grandchild);
                            
                        }
                        if (grandchild.Name == "email")
                        {
                            var email = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            location.Emails.Add(email);
                        }
                        if (grandchild.Name == "url")
                        {
                            var url = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            location.Urls.Add(url);
                        }
                        if (grandchild.Name == "remarks")
                        {

                            location.Remarks.Value = grandchild.InnerText;
                            location.Remarks.XPath = FindDeepestPath(grandchild);

                        }

                    }
                    Locations.Add(location);
                }
                if(child.Name == "revision-history")
                {
                    Revisions = new List<Revision>();
                   foreach(XmlNode mchild in child.ChildNodes)
                   {
                        var revision = new Revision();
                        revision.Properties = new List<Property>();
                        foreach (XmlNode grchild in mchild)
                        {
                            

                            if (grchild.Name == "title")
                            {
                                revision.Title = new Item();
                                revision.Title.Value = child.InnerText;
                                revision.Title.XPath = FindDeepestPath(grchild);
                            }

                            if (grchild.Name == "published")
                            {
                                revision.Published = new DateItem();

                                DateTime.TryParse(grchild.InnerText, out revision.Published.Date);
                                revision.Published.XPath = FindDeepestPath(grchild);

                            }

                            if (grchild.Name == "last-modified")
                            {
                                revision.LastModified = new DateItem();
                                DateTime.TryParse(grchild.InnerText, out revision.LastModified.Date);
                                revision.LastModified.XPath = FindDeepestPath(grchild);
                            }

                            if (grchild.Name == "version")
                            {
                                revision.Version = new Item();
                                revision.Version.Value = grchild.InnerText;
                                revision.Version.XPath = FindDeepestPath(grchild);
                            }

                            if (grchild.Name == "oscal-version")
                            {
                                revision.OSCALVersion = new Item();
                                revision.OSCALVersion.Value = grchild.InnerText;
                                revision.OSCALVersion.XPath = FindDeepestPath(grchild);
                            }

                            if (grchild.Name == "prop")
                            {

                                var prop = new Property();
                                prop.Name = grchild.Attributes[0].Value;
                                prop.Value = grchild.InnerText;
                                prop.XPath = FindDeepestPath(grchild);
                                revision.Properties.Add(prop);
                            }
                            if (grchild.Name == "remarks")
                            {
                                revision.Remarks = new Item();
                                revision.Remarks.Value = grchild.InnerText;
                                revision.Remarks.XPath = FindDeepestPath(grchild);
                            }
                           
                        }
                        Revisions.Add(revision);
                    }
                }
                if (child.Name == "party")
                {
                    var party = new Party();
                    party.Addresses = new List<Address>();
                    party.Emails = new List<Item>();
                    party.Phones = new List<Phone>();
                    party.UUID= child.Attributes[0].Value;
                    party.Type = child.Attributes[1].Value;

                    foreach (XmlNode grandchild in child)
                    {
                        if (grandchild.Name == "party-name")
                        {
                            party.Name.Value = grandchild.InnerText;
                            party.Name.XPath = FindDeepestPath(grandchild);
                        }
                        if (grandchild.Name == "short-name")
                        {
                            party.ShortName.Value = grandchild.InnerText;
                            party.ShortName.XPath = FindDeepestPath(grandchild);
                        }

                        if (grandchild.Name == "address")
                        {
                            var address = GetAddress(grandchild);
                            party.Addresses.Add(address);
                        }
                        if (grandchild.Name == "email")
                        {
                            var email = new Item();
                            email.Value = grandchild.InnerText;
                            email.XPath = FindDeepestPath(grandchild);
                            party.Emails.Add(email);
                        }
                        if (grandchild.Name == "phone")
                        {
                            var phone = new Phone();
                            phone.PhoneNumber.Value = grandchild.InnerText;
                            phone.PhoneNumber.XPath = FindDeepestPath(grandchild);
                            phone.PhoneType.Value = grandchild.Attributes[0].Value;
                            party.Phones.Add(phone);
                        }
                        if (grandchild.Name == "remarks")
                        {
                            party.Remarks.Value = grandchild.InnerText;
                            party.Remarks.XPath = FindDeepestPath(grandchild);
                        }

                    }

                    Parties.Add(party);
                }
                if (child.Name == "responsible-party")
                {
                    var responsibleparty = new ResponsibleParty();
                    responsibleparty.RoleID = child.Attributes[0].Value;
                    responsibleparty.PartyIDs = new List<Item>();
                    foreach (XmlNode grandchild in child)
                    {
                        if (grandchild.Name == "party-uuid")
                        {
                            var partId = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            responsibleparty.PartyIDs.Add(partId);
                        }

                        ResponsibleParties.Add(responsibleparty);
                    }
                }

            }

        }
        public Address GetAddress(XmlNode node)
        {
            var result = new Address();
            result.XPaths = new List<string>();
            if (node.ChildNodes[0].Name == "addr-line")
            {
                result.Line1 = new Item
                {
                    Value = node.ChildNodes[0].InnerText,
                    XPath = FindDeepestPath(node.ChildNodes[0])
                };
                result.XPaths.Add(result.Line1.XPath);
            }

            if (node.ChildNodes[1].Name == "addr-line")
            {
                result.Line2 = new Item
                {
                    Value = node.ChildNodes[1].InnerText,
                    XPath = FindDeepestPath(node.ChildNodes[1])
                };
                result.XPaths.Add(result.Line2.XPath);
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "city")
                {
                    result.City = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                    result.XPaths.Add(result.City.XPath);
                }

                if (child.Name == "state")
                {
                    result.State = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                    result.XPaths.Add(result.State.XPath);
                }

                if (child.Name == "postal-code")
                {
                    result.PostalCode = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                    result.XPaths.Add(result.PostalCode.XPath);
                }
                if (child.Name == "country")
                {
                    result.Country = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                    result.XPaths.Add(result.Country.XPath);
                }
            }
            return result;
        }

        public Person GetPerson(XmlNode node)
        {
            var result = new Person();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "person-name")
                {
                    result.Name = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "org-id")
                {
                    result.OrgID = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "address")
                {
                    result.Address = GetAddress(child);
                }
                if (child.Name == "email")
                {
                    result.Email = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "phone")
                {
                    result.Phone = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }


            }
            var temp = Organizations.Select(x => x).Where(x => x.OrgID.Value == result.OrgID.Value).FirstOrDefault();
            result.Organization = temp;
            return result;
        }

        public void FillPersonOrganization()
        {
            foreach (var party in Parties)
            {

            }
        }

        public Organization GetOrganization(XmlNode node)
        {
            var result = new Organization();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "org-name")
                {
                    result.Name = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "short-name")
                {
                    result.ShortName = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "org-id")
                {
                    result.OrgID = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "address")
                {
                    result.Address = GetAddress(child);
                }
            }


            return result;
        }

    }

   

    public struct Annotation
    {
        public string ParentID;
        public string Name { get; set; }
        public string ID { get; set; }
        public string NS { get; set; }
        public string Remarks { get; set; }

        public string Value { get; set; }
       
    }

 

    public struct RiskMetric
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string NS { get; set; }
        public string System { get; set; }
        public string Class { get; set; }
        public string Value { get; set; }
    }

    public struct SubjectReference
    {
        public string UUIDRef { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public struct RemediationOrigin
    {
        public string UUIDRef { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public struct MitigatingFactor
    {
        public string UUID { get; set; }
        public string ID { get; set; }
        public string ImplementationUUID { get; set; }

        public string Description { get; set; }

        public List<SubjectReference> SubjectReferences;

    }

    public struct Remediation
    {
        public string UUID { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<RemediationOrigin> RemediationOrigins;
        public List<Required> Requireds;

        public Schedule Schedule;
        public string Remarks { get; set; }
    }

    public struct SystemID
    {
        public string Identification { get; set; }
        public string Type { get; set; }
    }
    public struct TrackingEntry
    {
        public string UUID { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

        public string DateTimeStamp {get;set; }
        public string Description { get; set; }

        public List<Prop> Props;
        public List<Annotation> Annotations;       
        public string Remarks { get; set; }
    }


    public struct Required
    {
        public string UUID { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public List<SubjectReference> SubjectReferences;

        public string Remarks { get; set; }

    }

    public struct Prop
    {

        public string ParentID;
        public string Name { get; set; }
        public string ID { get; set; }
        public string NS { get; set; }
        public string Class { get; set; }
        public string Value { get; set; }
    }

    public struct Link
    {
        public string ParentID;
        public string HRef { get; set; }
        public string Rel { get; set; }
        public string MediaType { get; set; }
       
        public string MarkUpLine { get; set; }
       
    }

    public struct DocumentIdentifier
    {
        public string DocID { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public List<Prop> Props;

        public List<Link> Links;
    }
    public struct Revision
    {
        public Item Title;
        public DateItem Published;
        public DateItem LastModified;
        public Item Version;
        public Item OSCALVersion;
        public Item Remarks;
        public List<Property> Properties;
    }
    public struct DocRevision
    {
       
        public string RevisionID;
        public string Title { get; set; }
        public string Published { get; set; }
        public string LastModified { get; set; }
        public  string  Version { get; set; }
        public string OSCALVersion { get; set; }
        public string Remarks { get; set; }

        public List<Prop> Props;

        public List<Link> Links;
    }

    public struct Subject
    {
        public string Name { get; set; }
        public string Class { get; set; }

        public string All { get; set; }

        public string Description { get; set; }
 
        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }

    }

    public struct InventoryItem
    {
        public string UUID { get; set; }

        public string ID { get; set; }
        public string AssetID { get; set; }

        public string Description { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }
     
        public List<ResponsibleParty> ResponsibleRoles;
        public List<ImplementedComponent> implementedComponents;

    }

    public struct Schedule
    {
        public string UUID { get; set; }
        public string ID { get; set; }

        public List<Task> Tasks;
    }

    public struct Task
    {
        public string UUID { get; set; }
        public string ID { get; set; }

        public string Title { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }

        public string CompareTo { get; set; }
        public string Remarks { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public string Start { get; set; }
        public string End { get; set; }

        public List<string> RoleIds;
        public List<string> ActivityUUIDs;

        public List<string> PartyUUIDs;
        public List<string> LocationUUIDs;

    }

    public struct Result
    {
        public string ID { get; set; }

        public string UUID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<Finding> Findings;

        public List<Prop> Props;
        public List<Annotation> Annotations;

        public string Start { get; set; }
        public string End { get; set; }

        public string Remarks { get; set; }

    }

    public struct TestMethod
    {
        public string ID { get; set; }

        public string UUID { get; set; }

        public string Title { get; set; }


        public List<TestStep> TestSteps;

        public string Description { get; set; }

        public string CompareTo { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }
        

    }

    public struct TestStep
    {
        public string UUID { get; set; }
        public string ID { get; set; }
        public string TestMethodID { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }

        public string CompareTo { get; set; }
        public string Remarks { get; set; }
    
        public List<string> RoleIds;
        public List<string> PartyUUIDs;

    }

    public struct AuthorizedPrivilege
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string FunctionPerformed { get; set; }
    }

    public struct SAPUser
    {
        public string ID { get; set; }

        public string UUID { get; set; } 

        public string ShortName { get; set; }

        public string Title { get; set; }

        public List<string> RoleIDs;
        public List<AuthorizedPrivilege> AuthorizedPrivileges;
 
        public string Description { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }

        public string MainTag;

        public List<Role> Roles;
    }

    public struct Component
    {
        public string ID { get; set; }

        public string UUID { get; set; }
        public string ComponentType { get; set; }

        public string Title { get; set; }
       

        public string Description { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }

        public string State { get; set; }
        public string StateRemarks { get; set; }

        public List<ResponsibleParty> ResponsibleRoles;
    }


    public struct ImplementedComponent
    {
        public string ComponentID { get; set; }
        public string Use { get; set; }
        public string UUID { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links;

        public string Remarks { get; set; }

        public List<ResponsibleParty> ResponsibleRoles;
    }

    public struct RelevantEvidence
    {
        public string ID { get; set; } 
        public string HREF { get; set; }
        public string Description { get; set; }

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public string Remarks { get; set; }

    }
    public struct Finding
    {
        public string ID { get; set; }
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<Prop> Props;
        public List<Annotation> Annotations;
        public string DateTimeStamp { get; set; }

        public string ObjectiveID;

        public string ControlID;
        public string ObjectiveTitle;
        public string ObjectiveStatusDesc;
        public string ResultSystem;
        public string ResultValue;
        public string ImplementationStatusSystem;
        public string ImplementationStatusValue;
        public string ObjectiveStatusRemarks;
        public string ImplementationStatementUUIDs;
        public List<Observation> Observations;
        public List<Risk> Risks;
        public List<ThreatID> ThreatIDs;
        public List<string> PartyUUIDS;
        public string Remarks { get; set; }

    }

    public struct ThreatID
    {
        public string System { get; set; }
        public string URI { get; set; }
        public string Value { get; set; }
        public string ID { get; set; }
    }
    public struct Observation
    {
        public string ID { get; set; }
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<string> ObservationMethod;
        public List<string> ObservationType;
        public List<Item> Assessors;
        public List<SubjectReference> SubjectReferences;
        public List<RemediationOrigin> Origins;

        public List<RelevantEvidence> RelevantEvidences;
        public string Remarks { get; set; }
    }

    

    public struct Risk
    {
        public string ID { get; set; }
        public string UUID { get; set; }
        public string Title { get; set; }
        public string  Description { get; set; }

        public string RiskStatus { get; set; }

        public List<Prop> Props;
        public List<Annotation> Annotations;

        public List<RiskMetric> RiskMetrics;

        public List<MitigatingFactor> MitigatingFactors;

        public List<Remediation> Remediations;
        public List<TrackingEntry> TrackingEntries;

        public List<string> PartyUUIDs;

        public string Remarks { get; set; }

    }

    public struct Role
    {
        public string RoleID { get; set; }

        public Item Title;

        public string RoleTitle { get; set; }

        public string ShortName { get; set; }
       
        public string Description { get; set; }
        public string ElementTag;

        public List<Prop> Props;

        public List<Annotation> Annotations;

        public List<Link> Links; 

        public string Remarks { get; set; }

    }

    public struct DateItem
    {
        public DateTime Date;
        public string XPath;
    }

    public struct Item
    {
        public string Value;
        public string XPath;
    }

    public struct Location
    {
        public string UUID;
        public Item Title;
        public Address Address;
        public List<Item> Emails;
        public List<Phone> Phones;
        public List<Item> Urls;
        public Item Remarks;
    }
    public struct Address
    {
        public Item Line1;
        public Item Line2;
        public Item City;
        public Item State;
        public Item PostalCode;
        public Item Country;
        public List<string> XPaths;
    }
    public struct Organization
    {
        public Item Name;
        public Item OrgID;
        public Item ShortName;
        public Address Address;
    }

    public struct DocAddress
    {
        public string AddressID { get; set; }
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

    }

   

    public struct Person
    {
        public Item Name;
        public Item OrgID;
        public Organization Organization;
        public Item Email;
        public Item Phone;
        public Address Address;

        public string PersonName { get; set; }
        public string ShortName { get; set; }
        public string PersonOrgID { get; set; }

        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string PersonPhone { get; set; }

        public string PersonEmail { get; set; }

        public string LocationID { get; set; }

        public List<string> Emails;
        public List<string> Phones;
        public List<string> Url;
        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<Link> Links;

        public string Remarks { get; set; }
    }



    public struct DocLocation
    {
        public string LocationID { get; set; }
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }    

        public string UUID { get; set; }

        public List<string> Emails;
        public List<Phone> Phones;
        public List<string> Url;
        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<Link> Links;

        public string Remarks { get; set; }
    }

    public struct DocParty
    {
        public bool IsAPerson {  get
            {
                return OrgName != null && OrgName.Length == 0;
            }
        }
        public int Rank;
        public string UUID { get; set; }
        public string PartyID { get; set; }
        public string Name { get; set; }
        public string PartyType { get; set; }

        public string ExternalID { get; set; }
        public string ExternalType { get; set; }
        public string OrgName { get; set; }
        public string PersonName { get; set; }
        public string ShortName { get; set; }
        public string OrgID { get; set; }

        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public List<Location> Locations;
        public List<DocAddress> Addresses;
        public List<string> Emails;
        public List<string> MemberOfOrg;
        public List<string> LocationUUIDs;
        public List<Phone> Phones;
        public List<string> Url;
        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<Link> Links;

        public string Remarks { get; set; }
    }

    public struct Phone
    {
        public string Type;
        public string Number;
        public Item PhoneType;
        public Item PhoneNumber;
    }

    public struct Party
    {
        public string PartyID { get; set; }
        public string UUID;
        public string Type;
        public Item Name;      
        public Item ShortName;
        public List<Address> Addresses;
        public List<Item> Emails;
        public List<Phone> Phones;
       
        public Item Remarks;
        public Organization Organization;
        public Person Person;
    }

    public struct Citation
    {
        public string ID { get; set; }
        public string Target { get; set; }
        public string Title { get; set; }
    }

    public struct Resource
    {
        public string ID { get; set;}
        public string Desc { get; set; }
        public string FileName { get; set; }
        public string DataStream { get; set; }
    }

    public struct ResponsibleParty
    {
        public string RoleID { get; set; }
        public string PartyID { get; set; }
        public string PartyUUID { get; set; }
        public List<Item> PartyIDs;

        public List<Prop> Props;
        public List<Annotation> Annotations;
        public List<Link> Links;

        public string Remarks { get; set; }
    }

    public struct IndicatorRank
    {
        public int Rank;
        public string Indicator;
    }

    public struct  User
    {
        public string ID { get; set; }
        public string RoleID { get; set; }
        public string Title { get; set; }
        public string NS { get; set; }
        public string External { get; set; }
        public string Access { get; set; }
        public string SensitivityLevel { get; set; }
        public string AuthorizedPrivilegeName { get; set; }
        public string FunctionPerformed { get; set; }
    }
}
