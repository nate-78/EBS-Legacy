# EBS Legacy Code
This repo contains the core .NET applications that were in heavy use from 2015 - 2025.  The code is messy and has gone through many revisions in that time.  

This repo is not the original home of these applications. They were originally kept in separate SVN repos, and I have not imported that history over.  I've just copied the core applications and put them in this one repo, because I do not intend to run them or manage them from here -- I just want these to be accessible and together for reference as we build the next version of this application suite.  

This document details relevant history and workflow, provides a synopsis of the various applications, and explains the goals for where we want to go next.

## History

### Context
In 2015 I began building a custom software solution for a company called EBS (Employer Benefits Services).  EBS specializes in helping small businesses with their HR and benefits needs.  When the Affordable Care Act was passed (ACA), it mandated that companies of a certain size must submit forms to the IRS each year listing which of their employees and dependents were offered health insurance.  These forms are 1094-C (a "cover" form that details the company's information) and 1095-C (each 1095-C contains information for an employee and their dependents).

The IRS offers a platform called AIR (Affordable Care Act Information Returns).  This platform accepts the 1094-C and 1095-C submissions as XML files, and companies can either upload the files manually via a UI, or they can submit via a SOAP API service, which the IRS refers to as "A2A" for "application to application".

EBS hired me to build a custom software solution that would enable them to make these submissions on behalf of their clients. Even though I wrote the software, EBS is considered the "Transmitter" in the eyes of the IRS.  The applications belong to EBS -- I'm just the contractor who's created them.

My initial attempts at the software were to use the A2A option.  However, I was coding in .NET, and I eventually came to feel that the IRS's restrictions and security requirements simply wouldn't work with the way .NET compresses and signs SOAP submissions using an X.509 certificate.  I was never able to get my solution to work with A2A.  In the end, I rewrote the application to use the UI approach, and I had a headless browser submit via the UI channel programmatically.

However, by the second or third year of submissions, this approach no longer worked because of extra log in security implemented by the IRS.  So I began submitting the XML files manually via the UI channel, and this is how I've handled submissions ever since.

In the early years, the IRS had significant changes each filing year (the fields in the 1094-C and 1095-C forms would change, the format of the XML documents would change, etc), so revisions were always needed for the application.  However, there have been almost no changes in recent years, which has made maintenance a little easier.

But I have encountered different problems.  

### Workflow
EBS works with their clients to compile all of the necessary data, and this data is put into a spreadsheet.  The first worksheet details the data about that particular business entity (the data needed for the 1094-C file).  The second worksheet contains all of the data needed for the 1095-Cs.  

Early on, I learned that the spreadsheets were too large to let the web application process them in realtime.  We faced too many timeout issues in the user's session, so they were not able to see the Receipt ID returned by the IRS.  So I switched to an asynchronous approach, where the web interface would allow the employee at EBS to upload the spreadsheet, and then it would notify them via email when the submission was complete.

I had 2 or 3 "web jobs" deployed to Azure that would run every few minutes and look submitted files.  When it found one, it would process it.  Initially, the Selenium browser would then submit the XML files via the UI channel, document the Receipt ID, and notify the user of all the details.  Once the Selenium browser was no longer an option, the application would just create the XML files and store them in a "vault" that I created using storage on Azure.  The user would be notified that the files were ready (a manifest.xml file and an XML file containing the 1094-C and 1095-C files).  The user would then download those files from the vault and submit them via the IRS's UI channel.  They would manually enter the Receipt ID via a small form on the website, and that's how we handled submissions.

After a while, as my application got older, I was no longer able to create and deploy web jobs on Azure in the way that I had been.  So I eventually changed to my current workflow: EBS sends me the spreadsheets.  I submit them on the web.  Then I run my ACAfiling application locally to generate the files.  They are no longer saved in the Azure vault, but just saved on my laptop.  I then upload them to the IRS and manually track the Receipt IDs.

After submissions have been entered, we also have to check their statuses on the IRS's website.  This is also a manual job for us.

## The Applications

### EbsClassPkg
The EbsClassPkg contains most of the business logic needed for the other applications. It contains helper functions, connections to the SQL Server databases, email functions, etc.  Its most important file is `Controllers/ReportBuilder.cs`, as this file takes in the Excel spreadsheet and converts it to a business object.

### ACAfiling_Web
This application handles the web interface for uploading submissions, entering Receipt IDs and certain statuses, reviewing submitted statuses, accessing the vault, and any other necessary functions.  I plan to rebuild the web interface and simplify it, but I've included this code for reference and context.

### ACAfiling
This is the main workhorse application that takes the business object created by EbsClassPkg and converts it to all of the necessary XML files.  The string building done is this application works pretty well, even though it's probably not the preferred way to build XML. But it's an excellent reference, because its core competencies of taking the data and building accurate output acceptible to the IRS's UI channel is solid.  It was used for tax year 2024's data several months ago.

### ErrorImporter
When files are submitted to the IRS, it's rare to immediately get back a full "Accepted" response.  In most cases, we get "Accepted with Errors," which means the 1094-C file was accepted and many (or most) of the 1095-C files were accepted.  This means our documents were formatted correctly, and our data is mostly in good shape.  The IRS gives us an XML error report, and this application reads that report and updates our database with the appropriate info so EBS can know which employees had issues with their data and how we can resolve those.

### PdfCreatorWebJob
While this application used to be a web job, I now run it manually as a console app.  In addition to submitting the data to the IRS, EBS also prints and mails all 1095-C forms on behalf of their clients.  This application performs that function, using the same spreadsheet that will later be used for submitting the data.

## Goals and Next Steps
The manual nature of the current workflow and the great age of the applications is becoming untenable.  I'm worried that we will eventually reach a point where the applications can't do their jobs one year, and we'll be in deep trouble.  We need a fresh build that accomplishes these core bits of functionality:
1. Accept the same spreadsheet that we've been using.
2. Validate the fields (EBS asked us not to do this initially, but we need basic field validation to avoid stupid errors). I imagine we'll have to email the user when validation errors are encountered, rather than checking during the session.
3. Create the PDFs that will be mailed to the employees (what PdfCreatorWebJob currently does).
4. Submit the data to the IRS *using the A2A SOAP service channel*.
5. Either pull status updates from the IRS on a schedule or on demand if the user instigates it from our web application. We will use the SOAP channel for this.
6. Errors will be processed when the status is retrieved from the IRS.  The status will be maintained on dashboard in the website, but the submitting user will also be notified via email.

To achieve these goals, I think we should:
1. Form a meticulous architecture and execution plan.
2. Build an API in a different repo (C:\owensdev-git\EBS-v2-API) that will handle all of the backend function discussed above.
3. Build a front-end application in a different repo (C:\owensdev-git\EBS-v2-UI) that will serve as the interface for the users.

### Supporting Documents
**Note:** The IRS's documents about AIR, the IRS's XSD files, the IRS's 1095-C files, and the EBS spreadsheet that we use for submitting data can all be found in the `_documents` directory.  These documents and files should be used for context, for developing our plan of action, and for creating the API application. 

### Concerns and Open Questions
This set of applications is extremely important. This is a very large project (at least, to me it is), and it's imperative that we complete it correctly.  We need to make sure that ever step has been carefully planned, is carefully executed, and is thoroughly tested so we can have a high confidence that the applications are ready for use.

We should always discuss any major step in the plan or any significant decision.  And any time a decision will deviate from the way the code was approached previously, we should discuss.  While the current application is messy and outdated, it does still work accurately.

Here are my ideas about how to approach the project, but these are also open to revision if there are better ideas to consider:
- **API:** .NET for the API.  The reason for this is that I'm most familiar with .NET for backend coding.  I am open to Node.  I would also consider PHP or Python, but I think .NET will seem more straightforward to me, especially when errors are encountered.  That's also what the existing codebase is written in.  So we can change to something else, but should have very compelling reasons for doing so.
- **Hosting:** The applications will be hosted on Azure -- or the backend service will be at the very least. I would also consider Netlify for the front-end.
- **Front-end:** I'm leaning toward building the front-end in Astro.  I do not like React so will not build it in NextJS.  I do like Svelte, so SvelteKit can be considered.  But I would prefer to use Astro unless there's a very good reason not to.
- **Database:** The current database stores a lot of information, and I wonder if all of that is needed. I would be interested in setting up a new database for this project, and I would like to only store the absolute bare minimum of information.  The current project saves a lot of sensitive information about all of the employees it processes, but I don't know that we need all of that.  We really just need a way to identify which individuals (or at least which records) contain errors when the IRS gives us status updates, so that we can get the correct data from those individuals and submit corrections.  So if there's a simpler, smarter way to be able to make that determination, I'm all for it.