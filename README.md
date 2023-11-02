<h1>Customizing ASP.NET Core Identity</h1>

<p>
Since ASP.NET Core Identity is an extensible system, it is possible to create an ASP.NET Core Identity façade over an existing Microsoft SQL Membership.
Further, ASP.NET Core Identity is not restricted to a Microsoft SQL Server persistent data store.
</p>

<h2>This mini project will demonstrate these possibilities by:</h2>

<ul>
	<li>
	Persisting the Microsoft SQL Membership data in a Microsoft Access Database.
	The Microsoft Access (Jet Engine) Database is included in every Windows 32-bit operating system since Microsoft Windows 95 OSR 2,
	and this includes Windows 64-bit operating systems running 32-bit applications.
	The mini project should run as a 32-bit (x86) application on any Windows operating system that supports .NET Core 7.
	The Access database included in this mini project was created by importing the Microsoft SQL Membership tables into a blank database
	created by Microsoft Access for Microsoft 365 MSO v2310 and then saving it as a Access 2000 Database (*.mdb).
	</li>
	<li>
	Using the ASP.NET Core Identity SignInManager and UserManager to check and validate username and password credentials.
	</li>
</ul>

<p>
A Microsoft Test Project (using the Visual Studio 2022 “xUnit Test Project” template), is used to demonstrate the proof-of-concept,
because it offers more flexibility than a Console application, especially in terms of incremental concept development.
</p>

<p>The creation and validation of a Json Web Token (JWT) can also be found.</p>