# Attachmentmover

Attachement mover is a simple  solution to move file(s) from local folder to Dynamics 365 CE . Tool use metadriven approach to move the files.  The enclosed code samples provide a simple console app to move the file from local folder to  Dynamics 365 using 3 approaches 

1. Moving to Azure as staging source and move dynamics  
2. Moving to Staging entity in dynamics and 
3. Moving to share point as staging and then moving to Dynamics  

The file(s) can either been moved as annotation or file filed ,this is driven by the configuration.

The components in this open source consists  of
  1. Console App  -.Net core code to move the file usinn JSON meta data 
  2. Power Apps solution  - Power Automate flow to move the File from staging to dynamics  
