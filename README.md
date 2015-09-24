# AppProxyRedirector
Azure Active Directory App Proxy application aliasing via a single URL.

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## Why is this project needed?
When you publish an Azure App Proxy application it gets a URL assigned to it in the format `https://<name of app>-<name of tenant>.msappproxy.net/`. It is then possible to create a CNAME record in DNS, and upload an appropriate certificate for this CNAME so that the URL for the application is in the domain of your choice. However, what if you're publishing many applications within your organisation? Do you want to go through configuring a CNAME record and certificate for each application? What if there are hundreds of applications? This project can provide a solution.

Instead of configuring many applications, just deploy and configure this application in an Azure Web App (even the Free tier is fine), and it will then redirect all requests it receives to the App Proxy application URLs configured in your Azure Active Directory tenant, based on their names. For example, if you've deployed to the web app `https://redirectme.azurewebsites.net/` and you have an Azure App Proxy application published in your tenant named `MyCompanyApp`, then accessing the URL `https://redirectme.azurewebsites.net/mycompanyapp` will cause the user to be redirected to the actual published URL for that application. Now you can configure a single CNAME and certificate for the web app, and as you publish applications through App Proxy they will automatically be accessible via this redirection.

## Deployment and Configuration
*TODO*
