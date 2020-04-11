using System.Text.RegularExpressions;

namespace WCell.Util
{
    /// <summary>
    /// Edited by Domi for some performance gain
    /// 
    /// Implements an email validation utility class
    /// Validation based on code from
    /// http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp
    /// We added checks for length as defined at
    /// http://email.about.com/od/emailbehindthescenes/f/address_length.htm
    /// Validates an email address for proper syntax.
    /// </summary>
    /// <example>
    /// Validate an array of email addresses with the
    /// <see cref="T:WCell.Util.EmailAddressParser" /> class.
    /// <code>
    /// string[]             emails;
    /// EmailSyntaxValidator emailsyntaxvalidator;
    /// int                  countgood=0, countbad=0;
    /// 
    /// 
    /// //TODO: set emails string[] array
    /// 
    /// 
    /// //validate each email in the array
    /// foreach(string email in emails)
    /// {
    /// 	emailsyntaxvalidator = new EmailSyntaxValidator(email, true);
    /// 	if (emailsyntaxvalidator.IsValid)
    /// 	{
    /// 		countgood ++;
    /// 	}
    /// 	else
    /// 	{
    /// 		Console.WriteLine(email);
    /// 		countbad ++;
    /// 	}
    /// 	emailsyntaxvalidator = null;
    /// }
    /// 
    /// 
    /// Console.WriteLine("good: {0}\r\nbad : {1}", countgood, countbad);
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// Validates emails for proper syntax as detailed here:
    /// </para>
    /// <para>
    /// <A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
    /// </para>
    /// <para> </para>
    /// <para>
    /// Version Information
    /// </para>
    /// <para>
    /// 	    Email verification in general has had a checkered history at the ASP
    /// 	    Emporium. It took a while but I think we finally came up with something
    /// 	    good... Here's the short version history of all email validation
    /// 	    software from ASP Emporium...
    /// </para>
    /// <para>
    /// All future versions of email validation from ASPEmporium will be C# classes
    /// written for the .NET framework.
    /// </para>
    /// <para>
    /// 		10/2002 v4.0 (C#)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			Added new TLD (.int). Thanks to alex.wernhardt@web.de
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			Rebuilt from the ground up as a C# class that uses
    /// 			only regular expressions for string parsing.
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			Supports all the rules as detailed here:
    /// 			<A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
    /// 			This repairs all known issues in version 3.2 which
    /// 			was written in JScript for classic ASP.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		10/2002 v4.0 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			Added new TLD (.int). Thanks to alex.wernhardt@web.de
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			Supports all the rules as detailed here:
    /// 			<A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
    /// 			This repairs all known issues in version 3.2 which
    /// 			was written in JScript for classic ASP.
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			This is the last edition of email software that is
    /// 			written for classic ASP (JScript/VBScript).
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		2/2002 v3.2 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			fixed a problem that allows emails like test@mydomain.......com
    /// 			  to pass through. Thanks to g.falcone@mclink.it for letting
    /// 			  me know about it.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		11/2001 v3.1 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			added new tlds. thanks to alex.wernhardt@web.de for sending
    /// 			  me the list - http://www.icann.org/tlds/
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			new tlds: aero, biz, coop, info, museum, name, pro
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		9/2001  v3.0 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			fixed spaced email problem. thanks to mikael@perfoatris.com
    /// 			  for the report.
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			put the length check right in the function rather
    /// 			  than relying on a programmer to check length before
    /// 			  testing an email. thanks to eduardo.azambuja@uol.com.br for
    /// 			  bringing that to my attention.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		7/2001  v2.5 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			forgot the TLD (.gov). added now...
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			fixed @@ problem... thanks to davidchersg@yahoo.com for
    /// 			  letting me know that the problem was still there.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		5/2001  v2.0 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			added verification of all known TLDs as of
    /// 			  May 2001: http://www.behindtheurl.com/TLD/
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			added line to remove leading and trailing spaces before
    /// 			  testing email
    /// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200105060001
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			regular expression improvements by:
    /// 				Bj�rn Hansson  -  http://nytek.nu
    /// 			  you can view his emails here:
    /// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104180005
    /// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104090006
    /// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104090005
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			this email verification software replaces all other email
    /// 			  verification software at the ASP Emporium. VBScript versions
    /// 			  have been abandoned in favor of this JScript version.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		2/2001  v1.5 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			Regular Expression introduced to validate emails. Basically
    /// 			  a re-hashed version of the VBScript edition of IsEmail, aka
    /// 			  the EmailVerification object 3.0 (next line below)
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		12/2000 v3.0 (VBScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			EmailVerification Class released, resolving multiple domain
    /// 			  and user name problems.
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			Abandoned VBScript processing of emails in favor of regular
    /// 			  expressions.
    /// 		</description>
    /// 	</item>
    /// 	<item>
    /// 		<description>
    /// 			New VBScript class structure.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		8/2000  v1.0 (JScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			Initial Release of IsEmail for JScript is a lame function
    /// 			  that uses weak JScript inherent functions like indexOf...
    /// 			  This is essentually a copy of the vbscript edition of the
    /// 			  software, version 2, remembered on the next line below...
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		8/2000  v2.0 (VBScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			IsEmail function updated to resolve several issues but
    /// 			  multiple domains still pose a problem.
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		4/2000  v1.0 (VBScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			IsEmail function introduced
    /// 			  (used in the Simple Email Verification example)
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// <para>
    /// 		4/2000  v0.1 (VBScript)
    /// <list type="bullet">
    /// 	<item>
    /// 		<description>
    /// 			First email validation code at the ASP Emporium checks only
    /// 			  for an @ and a . (Used in the first version of the
    /// 			  autoresponder example)
    /// 		</description>
    /// 	</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class EmailAddressParser
    {
        public static readonly Regex AddressRegex =
            new Regex("^(.+?)\\@(.+?)$", RegexOptions.Singleline | RegexOptions.RightToLeft);

        public static readonly Regex BracketRegex =
            new Regex("^\\<*|\\>*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex TrimRegex =
            new Regex("^\\s*|\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex TLDomainRegex = new Regex("^((([a-z0-9-]+)\\.)+)[a-z]{2,6}$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex AnyDomainRegex =
            new Regex("^((([a-z0-9-]+)\\.)+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex DomainExtensionRegex = new Regex(
            "" + "\\.(" + "a[c-gil-oq-uwz]|" + "b[a-bd-jm-or-tvwyz]|" + "c[acdf-ik-orsuvx-z]|" + "d[ejkmoz]|" +
            "e[ceghr-u]|" + "f[i-kmorx]|" + "g[abd-ilmnp-uwy]|" + "h[kmnrtu]|" + "i[delm-oq-t]|" + "j[emop]|" +
            "k[eg-imnprwyz]|" + "l[a-cikr-vy]|" + "m[acdghk-z]|" + "n[ace-giloprtuz]|" + "om|" + "p[ae-hk-nrtwy]|" +
            "qa|" + "r[eouw]|" + "s[a-eg-ort-vyz]|" + "t[cdf-hjkm-prtvwz]|" + "u[agkmsyz]|" + "v[aceginu]|" + "w[fs]|" +
            "y[etu]|" + "z[admrw]|" + "com|" + "edu|" + "net|" + "org|" + "mil|" + "gov|" + "biz|" + "pro|" + "aero|" +
            "coop|" + "info|" + "name|" + "int|" + "museum" + ")$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly string inputemail;
        private readonly bool syntaxvalid;
        private string account;
        private string domain;

        /// <summary>Determines if an email has valid syntax</summary>
        /// <param name="email">the email to test</param>
        /// <param name="TLDrequired">indicates whether or not the
        /// email must end with a known TLD to be considered valid</param>
        /// <returns>boolean indicating if the email has valid syntax</returns>
        /// <remarks>
        /// Validates an email address specifying whether or not
        /// the email is required to have a TLD that is valid.
        /// </remarks>
        public static bool Valid(string email, bool TLDrequired)
        {
            return new EmailAddressParser(email, TLDrequired).IsValid;
        }

        /// <summary>
        /// Initializes a new instance of the EmailSyntaxValidator
        /// </summary>
        /// <param name="email">the email to test</param>
        /// <param name="TLDrequired">indicates whether or not the
        /// email must end with a known TLD to be considered valid</param>
        /// <remarks>
        /// The initializer creates an instance of the EmailSyntaxValidator
        /// class to validate a single email. You can specify whether or not
        /// the TLD is required and should be validated.
        /// </remarks>
        public EmailAddressParser(string email, bool TLDrequired)
        {
            this.inputemail = email;
            string email1 = EmailAddressParser.Trim(EmailAddressParser.RemoveBrackets(email));
            this.account = this.domain = "";
            if (!this.ParseAddress(email1) ||
                (this.Account.Length > 64 || this.Domain.Length > (int) byte.MaxValue ||
                 !this.DomainValid(TLDrequired)) || TLDrequired && !this.DomainExtensionValid())
                return;
            this.syntaxvalid = true;
        }

        /// <summary>
        /// Gets a value indicating whether or not the email address
        /// has valid syntax
        /// </summary>
        /// <remarks>
        /// This property returns a boolean indicating whether or not
        /// the email address has valid syntax as determined by the
        /// class.
        /// </remarks>
        /// <value>boolean indicating the validity of the email</value>
        public bool IsValid
        {
            get { return this.syntaxvalid; }
        }

        /// <summary>Get the domain part of the email address.</summary>
        /// <remarks>
        /// This property returns the domain part of the email
        /// address if and only if the email is considered valid
        /// by the class. Otherwise null is returned.
        /// </remarks>
        /// <value>string representing the domain of the email</value>
        public string Domain
        {
            get { return this.domain; }
        }

        /// <summary>Get the account part of the email address.</summary>
        /// <remarks>
        /// This property returns the account part of the email
        /// address if and only if the email is considered valid
        /// by the class. Otherwise null is returned.
        /// </remarks>
        /// <value>string representing the account of the email</value>
        public string Account
        {
            get { return this.account; }
        }

        /// <summary>Gets the email address as entered.</summary>
        /// <remarks>
        /// This property is filled regardless of the validity of the email.
        /// It contains the email as it was entered into the class.
        /// </remarks>
        /// <value>string representing the email address as entered</value>
        public string Address
        {
            get { return this.inputemail; }
        }

        /// <summary>separates email account from domain</summary>
        /// <param name="email">the email to parse</param>
        /// <returns>boolean indicating success of separation</returns>
        private bool ParseAddress(string email)
        {
            Match match = EmailAddressParser.AddressRegex.Match(email);
            if (!match.Success || match.Groups.Count < 2)
                return false;
            this.account = match.Groups[1].Value;
            this.domain = match.Groups[2].Value;
            return true;
        }

        /// <summary>removes outer brackets from an email address</summary>
        /// <param name="input">the email to parse</param>
        /// <returns>the email without brackets</returns>
        private static string RemoveBrackets(string input)
        {
            return EmailAddressParser.BracketRegex.Replace(input, "");
        }

        /// <summary>
        /// trims any leading and trailing white space from the email
        /// </summary>
        /// <param name="input">the email to parse</param>
        /// <returns>the email with no leading or trailing white space</returns>
        private static string Trim(string input)
        {
            return EmailAddressParser.TrimRegex.Replace(input, "");
        }

        private bool DomainValid(bool TLDrequired)
        {
            Regex regex;
            string input;
            if (TLDrequired)
            {
                regex = EmailAddressParser.TLDomainRegex;
                input = this.domain;
            }
            else
            {
                regex = EmailAddressParser.AnyDomainRegex;
                input = this.domain + ".";
            }

            return regex.IsMatch(input);
        }

        private bool DomainExtensionValid()
        {
            return EmailAddressParser.DomainExtensionRegex.IsMatch(this.domain);
        }
    }
}