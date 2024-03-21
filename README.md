# VR task
My solution consists of 5 main models:
1. **FileWatcher** - this module monitors changes in the target folder and adds new files to the processing queue.
1. **FileProcessQueue** - this queue is necessary for synchronizing and distributing files between loaders (you can add multiple file loaders).
1. **DataLoader** - the module receives information about the new file from the queue, verifies it and sends it to reader for processing.
1. **DataReader** - the module reads and parses information from the file, determines the type of a object and sends it to a database.
1. **DatabaseContext** - creates class objects and saves them to the database every 10 seconds (if there are new objects).