<h1>Customizing ASP.NET Core Identity</h1>
<p>
Since ASP.NET Core Identity is an extensible system, it is possible to create an ASP.NET Core Identity framework façade over an existing Microsoft SQL Membership
(a Visual Studio 2010 ASP.NET template implementation).<br/>
Further, ASP.NET Core Identity framework is not restricted to a Microsoft SQL Server persistent data store.
</p>

<p>This mini project will demonstrate these possibilities by:</p>

<ul>
	<li>
		Persisting the Microsoft SQL Membership data in a Microsoft Access database, rather than a Microsoft SQL Server database.
		The Microsoft Access database (Jet Engine), has been included in every Windows 32-bit operating system since Microsoft Windows 95 OSR 2,
		and this includes Windows 64-bit operating systems running 32-bit applications.<br/>
		This mini project will run if either <b>Microsoft Access 2010</b> or <b>Microsoft Access Database Engine 2010 Redistributable</b> are
		present on the computer.  You can download and install the necessary database software from here...<br/>
		<a href="https://www.microsoft.com/en-in/download/details.aspx?id=13255" target="_blank">Microsoft Access Database Engine 2010 Redistibutable</a>
	</li>
	<li>
		Using the ASP.NET Core Identity framework SignInManager and UserManager objects to check and validate username and password credentials.
	</li>
</ul>

<p>
A Microsoft Test Project (using the Visual Studio 2022 “xUnit Test Project” template), is used to demonstrate the proof-of-concept,
because it offers more flexibility than a Console application, especially in terms of incremental concept development.
</p>

<p>
	The creation and validation of a Json Web Token (JWT) can also be found (see also <a href="https://dev.azure.com/noctua-innovations/Customizing%20ASP.NET%20Core%20Identity/_git/CoreIdentity?path=/AspNetIdentity/Data/JsonWebToken.cs" target="_blank">Json Web Token</a>).
</p>