# Votes Export to CSV

This is a simple script that exports Cosmos Tables to CSV. 

We export them, so we can import CSV into BigQuery where we count the votes in order to compile agenda for the conference.

## Running

Install go 1.23+.

Replace `<PASTE CONNECTION STRING HERE>` in main.go with the actual connection string from Azure Portal.

Run:

```
go run main.go
```

That should create 3 files: 

* EloVotes.csv
* Submitters.csv
* Submissions.csv

## Big Query

For both dataset and query you'd need permissions given for your gmail.

The BigQuery dataset [could be found here](https://console.cloud.google.com/bigquery?project=dddmelbourne&ws=!1m11!1m3!3m2!1sdddmelbourne!2svoting2026!1m6!12m5!1m3!1sdddmelbourne!2saustralia-southeast1!3s66cc24e1-df88-4910-a9b7-b628fc312d73!2e1).

The query to produce list of talks [ordered by points is here](https://console.cloud.google.com/bigquery?project=dddmelbourne&ws=!1m11!1m3!3m2!1sdddmelbourne!2svoting2026!1m6!12m5!1m3!1sdddmelbourne!2saustralia-southeast1!3s66cc24e1-df88-4910-a9b7-b628fc312d73!2e1)

Once given create 3 new tables with respective names and choose CSV files as source:

* EloVotes
* Submitters
* Submissions

Run the query and go compile agenda please!