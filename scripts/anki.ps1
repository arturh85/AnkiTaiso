Set-Location (Split-Path -Path $Script:MyInvocation.MyCommand.Path)

$deckName = "Japanese - Core 2000"  # Set your deck name here
$outputFile = "../godot/src/data/ja/core2000.json" # Output file
$ankiConnectUrl = "http://127.0.0.1:8765"

# Properly format the query string
$queryString = "deck:" + '"' + $deckName + '"'
# Build the request to get all notes from the deck
$request = @{
  "action" = "findCards"
  "version" = 6
  "params" = @{ "query" = $queryString }
} | ConvertTo-Json -Depth 2

$response = Invoke-RestMethod -Uri $ankiConnectUrl -Method Post -Body $request -ContentType "application/json"

#Write-Host "Raw response from findNotes:"
#Write-Host ($response | ConvertTo-Json -Depth 3)
if (-not $response.result) {
  Write-Host "No cards found in deck '$deckName' or Anki is not running."
  # Request available decks
  $deckRequest = @{ "action" = "deckNames"; "version" = 6 } | ConvertTo-Json
  $deckResponse = Invoke-RestMethod -Uri $ankiConnectUrl -Method Post -Body $deckRequest -ContentType "application/json"

  if ($deckResponse.result) {
    Write-Host "Available decks:"
    $deckResponse.result | ForEach-Object { Write-Host " - <$_>" }
  }
  exit
}


$cards = $response.result
$cardCount = $cards.Count

# Get the note IDs for each card with a progress bar
$noteIds = @()
$i = 0
foreach ($cardId in $cards) {
  $i++
  Write-Progress -Activity "Fetching card details" -Status "Processing card $i of $cardCount" -PercentComplete (($i / $cardCount) * 100)

  $cardRequest = @{
    "action" = "cardsInfo"
    "version" = 6
    "params" = @{ "cards" = @($cardId) }
  } | ConvertTo-Json -Depth 2

  $cardResponse = Invoke-RestMethod -Uri $ankiConnectUrl -Method Post -Body $cardRequest -ContentType "application/json"

  if ($cardResponse.result) {
    $noteIds += $cardResponse.result[0].note
  }
}

# Get the field values for each note with a progress bar
$outputData = @()
$noteCount = $noteIds.Count
$i = 0
foreach ($noteId in $noteIds | Select-Object -Unique) {
  $i++
  Write-Progress -Activity "Fetching note details" -Status "Processing note $i of $noteCount" -PercentComplete (($i / $noteCount) * 100)

  $noteRequest = @{
    "action" = "notesInfo"
    "version" = 6
    "params" = @{ "notes" = @($noteId) }
  } | ConvertTo-Json -Depth 2

  $noteResponse = Invoke-RestMethod -Uri $ankiConnectUrl -Method Post -Body $noteRequest -ContentType "application/json"

  if ($noteResponse.result) {
    $fields = $noteResponse.result[0].fields
    $vocabularyKana = $fields."Vocabulary-Kana".value
    $vocabularyEnglish = $fields."Vocabulary-English".value
    $vocabularyKanji = $fields."Vocabulary-Kanji".value
    $vocabularyAudio = $fields."Vocabulary-Audio".value
    if ($vocabularyAudio -match "\[audio:(.+?)\]") {
      $vocabularyAudio = $matches[1]
    }
    if ($vocabularyAudio -match "") {
      $vocabularyAudio = null
    }
    $outputData += [PSCustomObject]@{
      "prompt" = $vocabularyKana.Replace("&nbsp;", " "),
      "title" = $vocabularyKanji.Replace("&nbsp;", " "),
      "translation" = $vocabularyEnglish.Replace("&nbsp;", " "),
      "audioFilename" = $vocabularyEnglish.Replace("&nbsp;", " ")
    }
  }
}
# Write to output JSON file
$outputData | ConvertTo-Json -Depth 2 | Set-Content -Path $outputFile -Encoding UTF8

Write-Host "Export completed: $outputFile"
