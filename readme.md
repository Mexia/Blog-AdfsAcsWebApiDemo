Federated identity using ADFS, ACS and ASP.NET Web API
======================================================

A sample client console application, and ASP.NET WebAPI REST service, that federates Active Directory identity from client to server, using ADFS and ACS.

Most scenarios deal with applying the WS-Trust active federation protocol with SOAP based services; basically the client uses WS-Trust to obtain a SAML token containing the claims it needs to access the web service.

In this post we deal with applying a similar approach, but specifically for REST based services. In a RESTful world, the WS-Trust protocol and SAML token format, is exchanged for the OAuth protocol and the Simple Web Token (SWT) token format. The client will obtain this SWT token from Windows Azure Access Control services (ACS) v2.


### Sequence of events ###

1.	Service Consumer obtains SAML token from ADFS (Active Directory Federation Services).
2.	The Service Consumer presents its SAML token to ACS’s OAuth integration endpoint.
3.	ACS resolves the relying party, and applies any associated rule groups to map claims, into a SWT token. ACS digitally signs this token.
4.	ACS returns the SWT token, to the Service Consumer.
5.	The Service Consumer prepares the HTTP request for the REST service, injecting the SWT token into the Authentication header, and sends the request.
6.	The REST service receives the request and inspects the SWT token. The REST service by having an awareness of the signing-key that ACS uses, can verify the token did in-fact come from ACS, and can be trusted, by computing Hash-based Message Authentication Code (HMAC) by using the SHA256 hash function and this signing-key.
7.	If the REST service is convinced the token is trusted, can enumerate and read the claims contained in the token, to determine whether it should proceed.

Credits to Carlos Sardo for posting up [TokenValidationHandler](http://code.msdn.microsoft.com/windowsazure/MVC4-Web-API-With-SWT-232d69da) on MSDN, that inspects the SWT token on the Web API request.

For more information, please refer to the accompanied [blog post](www.mexia.com.au/mexia-blog/).